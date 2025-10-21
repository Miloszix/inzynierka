#include <stdbool.h>
#include <stdio.h>
#include <string.h>

#include "esp_log.h"
#include "nvs_flash.h"
#include "sdkconfig.h"

#include <freertos/FreeRTOS.h>
#include <freertos/task.h>

#include "nimble/nimble_port.h"
#include "nimble/nimble_port_freertos.h"
#include "host/ble_hs.h"    // This header usually brings in most core definitions
#include "host/util/util.h" // For general BLE utilities, like address formatting if not directly in ble_hs.h


// --- Configuration ---
#define TAG "BLE_GATEWAY"
#define MANUFACTURER_ID_FILTER 0x1234 // The manufacturer ID your beacon uses

// Define BLE_ADDR_STR_LEN if not provided by headers (good fallback)
#ifndef BLE_ADDR_STR_LEN
#define BLE_ADDR_STR_LEN 18 // "XX:XX:XX:XX:XX:XX\0"
#endif

// This struct defines the format of your sensor data inside the manufacturer data
// It should match how your beacon packs the data (excluding the 2-byte MFG ID).
typedef struct __attribute__((packed)) {
    int16_t temperature_raw; // temp * 100 (e.g., 2345 for 23.45)
    uint16_t humidity_raw;   // hum * 100 (e.g., 5678 for 56.78)
    uint16_t pressure_raw;   // pressure / 10 (e.g., 101 for 1013.25)
} sensor_data_t;


// --- Private Function Declarations ---
static void ble_app_on_sync(void);
static void ble_app_on_reset(int reason);
static int ble_gap_event(struct ble_gap_event *event, void *arg);
static void start_scan(void);

// Helper function to format BLE address (our custom fallback)
static void format_ble_addr(const ble_addr_t *addr, char *str);

// --- Global Variables ---
static uint8_t own_addr_type;


// --- Main Application Entry Point ---
void app_main(void) {
    esp_err_t ret;

    // Initialize NVS flash (required for BLE)
    ret = nvs_flash_init();
    if (ret == ESP_ERR_NVS_NO_FREE_PAGES || ret == ESP_ERR_NVS_NEW_VERSION_FOUND) {
        ESP_ERROR_CHECK(nvs_flash_erase());
        ret = nvs_flash_init();
    }
    ESP_ERROR_CHECK(ret);

    // Initialize NimBLE host stack
    ret = nimble_port_init();
    if (ret != ESP_OK) {
        ESP_LOGE(TAG, "Failed to init NimBLE (err %d)", ret);
        return;
    }

    // Configure NimBLE host callbacks
    ble_hs_cfg.reset_cb = ble_app_on_reset;
    ble_hs_cfg.sync_cb = ble_app_on_sync;
    ble_hs_cfg.store_status_cb = ble_store_util_status_rr; // For BLE bonding if needed later

    // Start the NimBLE host task (this runs the BLE stack in a FreeRTOS task)
    nimble_port_run();
}


// --- NimBLE Host Callbacks ---
/**
 * Called when the BLE host stack has been reset.
 * @param reason The reason for the reset.
 */
static void ble_app_on_reset(int reason) {
    ESP_LOGE(TAG, "BLE Host reset; reason=%d", reason);
}

/**
 * Called when the BLE host stack has synced with the controller.
 */
static void ble_app_on_sync(void) {
    ESP_LOGI(TAG, "BLE Host Synced. Inferring address and starting scan.");

    int rc;

    // Ensure we have a valid BLE identity address
    rc = ble_hs_util_ensure_addr(0);
    assert(rc == 0); // Assert if failed (critical error)

    // Infer the best address type to use for scanning
    rc = ble_hs_id_infer_auto(0, &own_addr_type);
    assert(rc == 0); // Assert if failed

    // Start scanning for advertising packets
    start_scan();
}


// --- BLE Scan Logic ---
/**
 * Starts a continuous BLE passive scan.
 */
static void start_scan(void) {
    struct ble_gap_disc_params disc_params;
    int rc;

    // Configure scan parameters for passive, continuous scanning
    disc_params.itvl = BLE_GAP_SCAN_FAST_INTERVAL_MIN; // Scan interval (e.g., ~20ms)
    disc_params.window = BLE_GAP_SCAN_FAST_WINDOW;     // Scan window (e.g., ~10ms)
    disc_params.filter_policy = 0; // No whitelist filter
    disc_params.limited = 0; // 0 for continuous, 1 for limited duration scan
    disc_params.passive = 1; // 1 for passive scan (don't send scan requests)
    disc_params.filter_duplicates = 1; // 1 to filter duplicate advertising reports from the same device

    ESP_LOGI(TAG, "Starting continuous BLE passive scan...");

    rc = ble_gap_disc(own_addr_type, BLE_HS_FOREVER, &disc_params,
                      ble_gap_event, NULL); // BLE_HS_FOREVER for indefinite scan
    if (rc != 0) {
        ESP_LOGE(TAG, "Error starting scan: %d", rc);
    }
}


// --- BLE GAP Event Handler ---
/**
 * Handles various BLE GAP events, primarily device discovery.
 * @param event The GAP event structure.
 * @param arg   User-defined argument.
 * @return 0 on success.
 */
static int ble_gap_event(struct ble_gap_event *event, void *arg) {
    // THIS IS THE CRITICAL CHANGE: Accessing event->disc.fields directly again.
    // If ble_hs_adv_fields_rsp_init_from_buf is truly missing, this must be it.
    const struct ble_hs_adv_fields *fields; // Pointer to the advertising fields

    switch (event->type) {
    case BLE_GAP_EVENT_DISC: {
    struct ble_hs_adv_fields fields;
    int rc = ble_hs_adv_parse_fields(&fields, event->disc.data, event->disc.length_data);
    if (rc != 0) {
        ESP_LOGW(TAG, "Failed to parse advertisement from device.");
        return 0;
    }

    char addr_str[BLE_ADDR_STR_LEN];
    format_ble_addr(&event->disc.addr, addr_str);

    if (fields.mfg_data && fields.mfg_data_len >= (2 + sizeof(sensor_data_t))) {
        uint16_t mfg_id = (fields.mfg_data[1] << 8) | fields.mfg_data[0];

        if (mfg_id == MANUFACTURER_ID_FILTER) {
            ESP_LOGI(TAG, "--- Beacon from %s (RSSI: %d dBm) ---", addr_str, event->disc.rssi);

            const sensor_data_t *sensor_data = (const sensor_data_t *)(fields.mfg_data + 2);
            float temperature = sensor_data->temperature_raw / 100.0f;
            float humidity = sensor_data->humidity_raw / 100.0f;
            float pressure = sensor_data->pressure_raw * 10.0f;

            ESP_LOGI(TAG, "  Temperature: %.2f C", temperature);
            ESP_LOGI(TAG, "  Humidity:    %.2f %%", humidity);
            ESP_LOGI(TAG, "  Pressure:    %.2f hPa", pressure);

            if (fields.name && fields.name_len > 0) {
                char dev_name[fields.name_len + 1];
                memcpy(dev_name, fields.name, fields.name_len);
                dev_name[fields.name_len] = '\0';
                ESP_LOGI(TAG, "  Device Name: %s", dev_name);
            }
        }
    }
    break;
    }

    case BLE_GAP_EVENT_DISC_COMPLETE: // Scan completed (only if disc_params.limited was 1)
        ESP_LOGI(TAG, "Scan complete; reason=%d", event->disc_complete.reason);
        // If you were doing a limited scan, you might restart it here:
        // start_scan();
        break;

    default:
        // Other GAP events (e.g., connection-related) are ignored for this simple scanner
        break;
    }
    return 0; // Return 0 to indicate event handled
}

// Custom helper function to format BLE address
// This is a fallback if nimble's ble_addr_val_to_str isn't easily accessible/defined
static void format_ble_addr(const ble_addr_t *addr, char *str) {
    if (addr == NULL || str == NULL) {
        return;
    }
    // NimBLE stores address in little-endian byte order (LSB first for MAC address).
    // When printing a MAC address, it's typically MSB first.
    sprintf(str, "%02X:%02X:%02X:%02X:%02X:%02X",
            addr->val[5], addr->val[4], addr->val[3],
            addr->val[2], addr->val[1], addr->val[0]);
}


// Required by NimBLE for store configuration (even if not strictly using bonding)
void ble_store_config_init(void) {
    // This is a placeholder for actual store configuration if bonding is used.
    // For simple scanning, it's not strictly necessary to do anything here,
    // but the function must exist if referenced by ble_hs_cfg.
}