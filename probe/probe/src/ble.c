#include "ble.h"
#include "esp_log.h"
#include "freertos/FreeRTOS.h"
#include "freertos/task.h"

#include "nimble/nimble_port.h"
#include "nimble/nimble_port_freertos.h"
#include "host/ble_hs.h"
#include "services/gap/ble_svc_gap.h"
#include "services/gatt/ble_svc_gatt.h"

#define SENSOR_SERVICE_UUID        0x181A
#define CHAR_UUID_PRESSURE         0x2A6D
#define CHAR_UUID_TEMPERATURE      0x2A6E
#define CHAR_UUID_HUMIDITY         0x2A6F

static const char *TAG = "BLE";

// --- Dane pomiarowe ---
static float temp_val = 0, hum_val = 0, press_val = 0;

// --- Prototypy ---
static void ble_app_advertise(void);
static void ble_host_task(void *param);
static int ble_gap_event_cb(struct ble_gap_event *event, void *arg);

// =============================================================
// 🔹 AKTUALIZACJA DANYCH
// =============================================================
void ble_update_measurements(float temperature, float humidity, float pressure) {
    temp_val  = temperature;
    hum_val   = humidity;
    press_val = pressure;
}

// =============================================================
// 🔹 CALLBACK ODCZYTU CHARAKTERYSTYK
// =============================================================
static int ble_gatt_read(uint16_t conn_handle, uint16_t attr_handle,
                         struct ble_gatt_access_ctxt *ctxt, void *arg) {
    uint16_t uuid = ble_uuid_u16(ctxt->chr->uuid);
    char buf[16];
    float val = 0;

    switch (uuid) {
        case CHAR_UUID_TEMPERATURE: val = temp_val;  break;
        case CHAR_UUID_HUMIDITY:    val = hum_val;   break;
        case CHAR_UUID_PRESSURE:    val = press_val; break;
        default:
            return BLE_ATT_ERR_UNLIKELY;
    }

    // Zamiana na tekst
    int len = snprintf(buf, sizeof(buf), "%.2f", val);
    os_mbuf_append(ctxt->om, buf, len);

    return 0;
}

// =============================================================
// 🔹 DEFINICJA SERWISU I CHARAKTERYSTYK
// =============================================================
static const struct ble_gatt_svc_def gatt_svr_svcs[] = {
    {
        .type = BLE_GATT_SVC_TYPE_PRIMARY,
        .uuid = BLE_UUID16_DECLARE(SENSOR_SERVICE_UUID),
        .characteristics = (struct ble_gatt_chr_def[]) {
            {
                .uuid = BLE_UUID16_DECLARE(CHAR_UUID_TEMPERATURE),
                .flags = BLE_GATT_CHR_F_READ,
                .access_cb = ble_gatt_read
            },
            {
                .uuid = BLE_UUID16_DECLARE(CHAR_UUID_HUMIDITY),
                .flags = BLE_GATT_CHR_F_READ,
                .access_cb = ble_gatt_read
            },
            {
                .uuid = BLE_UUID16_DECLARE(CHAR_UUID_PRESSURE),
                .flags = BLE_GATT_CHR_F_READ,
                .access_cb = ble_gatt_read
            },
            { 0 } // koniec listy
        }
    },
    { 0 } // koniec usług
};

// =============================================================
// 🔹 CALLBACK ZDARZEŃ BLE (np. połączenie/rozłączenie)
// =============================================================
static int ble_gap_event_cb(struct ble_gap_event *event, void *arg) {
    switch (event->type) {
        case BLE_GAP_EVENT_CONNECT:
            if (event->connect.status == 0) {
                ESP_LOGI(TAG, "Connected to central");
            } else {
                ESP_LOGW(TAG, "Connection failed; restarting advertisement");
                ble_app_advertise();
            }
            break;

        case BLE_GAP_EVENT_DISCONNECT:
            ESP_LOGI(TAG, "Disconnected; restarting advertisement");
            ble_app_advertise();
            break;

        case BLE_GAP_EVENT_ADV_COMPLETE:
            ESP_LOGI(TAG, "Advertisement complete; restarting");
            ble_app_advertise();
            break;

        default:
            break;
    }
    return 0;
}

// =============================================================
// 🔹 REKLAMOWANIE URZĄDZENIA
// =============================================================
static void ble_app_advertise(void) {
    struct ble_hs_adv_fields fields = {0};

    fields.flags = BLE_HS_ADV_F_DISC_GEN | BLE_HS_ADV_F_BREDR_UNSUP;

    const char *name = ble_svc_gap_device_name();
    fields.name = (uint8_t *)name;
    fields.name_len = strlen(name);
    fields.name_is_complete = 1;

    ble_uuid16_t svc_uuid = BLE_UUID16_INIT(SENSOR_SERVICE_UUID);
    fields.uuids16 = &svc_uuid;
    fields.num_uuids16 = 1;
    fields.uuids16_is_complete = 1;

    int rc = ble_gap_adv_set_fields(&fields);
    if (rc != 0) {
        ESP_LOGE(TAG, "ble_gap_adv_set_fields err: %d", rc);
        return;
    }

    struct ble_gap_adv_params adv_params = {0};
    adv_params.conn_mode = BLE_GAP_CONN_MODE_UND;
    adv_params.disc_mode = BLE_GAP_DISC_MODE_GEN;

    rc = ble_gap_adv_start(BLE_OWN_ADDR_PUBLIC, NULL, BLE_HS_FOREVER,
                           &adv_params, ble_gap_event_cb, NULL);
    if (rc == 0)
        ESP_LOGI(TAG, "Advertising started");
    else
        ESP_LOGE(TAG, "Adv start error: %d", rc);
}

// =============================================================
// 🔹 CALLBACK SYNC BLE HOSTA
// =============================================================
static void ble_app_on_sync(void) {
    int rc;
    uint8_t addr_val[6];

    rc = ble_hs_id_infer_auto(0, &addr_val[0]);
    assert(rc == 0);

    ble_hs_id_copy_addr(BLE_ADDR_PUBLIC, addr_val, NULL);
    ESP_LOGI(TAG, "Device address: %02X:%02X:%02X:%02X:%02X:%02X",
             addr_val[5], addr_val[4], addr_val[3],
             addr_val[2], addr_val[1], addr_val[0]);

    ble_svc_gap_init();
    ble_svc_gatt_init();
    ble_svc_gap_device_name_set("ESP32-EnvSensor");

    ble_gatts_count_cfg(gatt_svr_svcs);
    ble_gatts_add_svcs(gatt_svr_svcs);

    rc = ble_gatts_start();
    if (rc != 0) {
        ESP_LOGE(TAG, "ble_gatts_start failed: %d", rc);
        return;
    }

    ESP_LOGI(TAG, "GATT services registered");
    ble_app_advertise();
}

// =============================================================
// 🔹 GŁÓWNE ZADANIE HOSTA BLE
// =============================================================
static void ble_host_task(void *param) {
    nimble_port_run();
    nimble_port_freertos_deinit();
}

// =============================================================
// 🔹 INICJALIZACJA BLE
// =============================================================
esp_err_t ble_init(void) {
    int rc = nimble_port_init();
    if (rc != 0) {
        ESP_LOGE(TAG, "nimble_port_init failed: %d", rc);
        return ESP_FAIL;
    }

    ble_hs_cfg.sync_cb = ble_app_on_sync;
    nimble_port_freertos_init(ble_host_task);

    ESP_LOGI(TAG, "NimBLE initialized");
    return ESP_OK;
}
