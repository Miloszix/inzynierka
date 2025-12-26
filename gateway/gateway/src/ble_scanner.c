#include <stdbool.h>
#include <stdio.h>
#include <string.h>

#include "esp_log.h"
#include "sdkconfig.h"

#include "nimble/nimble_port.h"
#include "nimble/nimble_port_freertos.h"
#include "host/ble_hs.h"
#include "host/util/util.h"

#include "config.h"
#include "ble_scanner.h"
#include "data_sender.h"

// --- Configuration ---
#define TAG "BLE_SCANNER"

#ifndef BLE_ADDR_STR_LEN
#define BLE_ADDR_STR_LEN 18
#endif

typedef struct __attribute__((packed))
{
    int16_t temperature_raw;
    uint16_t humidity_raw;
    uint16_t pressure_raw;
} sensor_data_t;

// --- Private Function Declarations ---
static void ble_app_on_sync(void);
static void ble_app_on_reset(int reason);
static int ble_gap_event(struct ble_gap_event *event, void *arg);
static void start_scan(void);
static void format_ble_addr(const ble_addr_t *addr, char *str);
void ble_host_task(void *param);

// --- Global ---
static uint8_t own_addr_type;

// --- PUBLIC API ---
void ble_init(void)
{
    int rc;

    rc = nimble_port_init();
    if (rc != 0)
    {
        ESP_LOGE(TAG, "Failed to init NimBLE (rc=%d)", rc);
        return;
    }

    ble_hs_cfg.reset_cb = ble_app_on_reset;
    ble_hs_cfg.sync_cb = ble_app_on_sync;
    ble_hs_cfg.store_status_cb = ble_store_util_status_rr;

    nimble_port_freertos_init(ble_host_task);
}

void ble_host_task(void *param)
{
    nimble_port_run();
    nimble_port_freertos_deinit();
}

// --- Callbacks ---
static void ble_app_on_reset(int reason)
{
    ESP_LOGE(TAG, "BLE Host reset; reason=%d", reason);
}

static void ble_app_on_sync(void)
{
    ESP_LOGI(TAG, "BLE synced, starting scan");

    int rc = ble_hs_util_ensure_addr(0);
    assert(rc == 0);

    rc = ble_hs_id_infer_auto(0, &own_addr_type);
    assert(rc == 0);

    start_scan();
}

// --- Scanning ---
static void start_scan(void)
{
    struct ble_gap_disc_params params = {
        .itvl = 0x10,
        .window = 0x10,
        .filter_policy = 0,
        .limited = 0,
        .passive = 1,
        .filter_duplicates = 1 // odbierasz jeden pakiet
    };

    ESP_LOGI(TAG, "Starting BLE scan");

    int rc = ble_gap_disc(own_addr_type, BLE_HS_FOREVER,
                          &params, ble_gap_event, NULL);

    if (rc != 0)
    {
        ESP_LOGE(TAG, "ble_gap_disc failed: %d", rc);
    }
}

// --- Event Handler ---
static int ble_gap_event(struct ble_gap_event *event, void *arg)
{
    switch (event->type)
    {

    case BLE_GAP_EVENT_DISC:
    {
        struct ble_hs_adv_fields fields;
        int rc = ble_hs_adv_parse_fields(&fields, event->disc.data, event->disc.length_data);

        if (rc != 0)
            return 0;

        char addr_str[BLE_ADDR_STR_LEN];
        format_ble_addr(&event->disc.addr, addr_str);

        if (fields.mfg_data &&
            fields.mfg_data_len >= (2 + sizeof(sensor_data_t)))
        {

            uint16_t mfg_id = (fields.mfg_data[1] << 8) |
                              fields.mfg_data[0];

            if (mfg_id == MANUFACTURER_ID_FILTER)
            {

                const sensor_data_t *data =
                    (const sensor_data_t *)(fields.mfg_data + 2);

                float t = data->temperature_raw / 100.0f;
                float h = data->humidity_raw / 100.0f;
                float p = data->pressure_raw / 10.0f;

                ESP_LOGI(TAG, "Beacon from %s", addr_str);
                ESP_LOGI(TAG, "  Temp: %.2f C", t);
                ESP_LOGI(TAG, "  Hum:  %.2f %%", h);
                ESP_LOGI(TAG, "  Press %.2f hPa", p);

                data_sender_handle_sensor(addr_str, t, h, p);
            }
        }
    }
    break;

    default:
        break;
    }

    return 0;
}

// --- Address formatting ---
static void format_ble_addr(const ble_addr_t *addr, char *str)
{
    sprintf(str, "%02X:%02X:%02X:%02X:%02X:%02X",
            addr->val[5], addr->val[4], addr->val[3],
            addr->val[2], addr->val[1], addr->val[0]);
}
