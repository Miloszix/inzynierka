#include "ble.h"

#include <stdbool.h>
#include <stdio.h>
#include <string.h>

/* ESP APIs */
#include "esp_log.h"
#include "nvs_flash.h"
#include "sdkconfig.h"
#include "esp_err.h"

/* FreeRTOS APIs */
#include <freertos/FreeRTOS.h>
#include <freertos/task.h>

/* NimBLE stack APIs */
#include "host/ble_hs.h"
#include "host/ble_uuid.h"
#include "host/util/util.h"
#include "nimble/ble.h"
#include "nimble/nimble_port.h"
#include "nimble/nimble_port_freertos.h"

#define BLE_GAP_APPEARANCE_GENERIC_TAG 0x0200
#define MANUFACTURER_ID 0x1234

// sensor data
static float g_temp = 0.0f;
static float g_hum = 0.0f;
static float g_press = 0.0f;

void ble_store_config_init(void);
void start_advertising(uint32_t duration_ms);

static void on_stack_reset(int reason);
static void on_stack_sync(void);
static void nimble_host_config_init(void);
static void nimble_host_task(void *param);

static uint8_t own_addr_type;
static uint8_t addr_val[6] = {0};

volatile bool ble_synced = false;

inline static void format_addr(char *addr_str, uint8_t addr[])
{
    sprintf(addr_str, "%02X:%02X:%02X:%02X:%02X:%02X",
            addr[0], addr[1], addr[2], addr[3], addr[4], addr[5]);
}

static void fill_sensor_data(uint8_t *buf)
{
    int16_t temp = (int16_t)(g_temp * 100);
    uint16_t hum = (uint16_t)(g_hum * 100);
    uint16_t pres = (uint16_t)(g_press * 10);

    buf[0] = temp & 0xFF;
    buf[1] = (temp >> 8) & 0xFF;
    buf[2] = hum & 0xFF;
    buf[3] = (hum >> 8) & 0xFF;
    buf[4] = pres & 0xFF;
    buf[5] = (pres >> 8) & 0xFF;
}

static int ble_gap_event_handler(struct ble_gap_event *event, void *arg)
{
    switch (event->type)
    {
    case BLE_GAP_EVENT_ADV_COMPLETE:
        ESP_LOGI(TAG, "BLE: ADV_COMPLETE (reason=%d)", event->adv_complete.reason);
        break;
    default:
        break;
    }
    return 0;
}

void start_advertising(uint32_t duration_ms)
{
    int rc;
    const char *name;
    struct ble_hs_adv_fields adv_fields = {0};
    struct ble_hs_adv_fields rsp_fields = {0};
    struct ble_gap_adv_params adv_params = {0};

    adv_fields.flags = BLE_HS_ADV_F_DISC_GEN | BLE_HS_ADV_F_BREDR_UNSUP;

    name = ble_svc_gap_device_name();
    adv_fields.name = (uint8_t *)name;
    adv_fields.name_len = strlen(name);
    adv_fields.name_is_complete = 1;

    uint8_t mfg_data[8];
    mfg_data[0] = MANUFACTURER_ID & 0xFF;
    mfg_data[1] = (MANUFACTURER_ID >> 8) & 0xFF;
    fill_sensor_data(&mfg_data[2]);

    adv_fields.mfg_data = mfg_data;
    adv_fields.mfg_data_len = sizeof(mfg_data);

    adv_fields.tx_pwr_lvl = BLE_HS_ADV_TX_PWR_LVL_AUTO;
    adv_fields.tx_pwr_lvl_is_present = 1;

    adv_fields.appearance = BLE_GAP_APPEARANCE_GENERIC_TAG;
    adv_fields.appearance_is_present = 1;

    rc = ble_gap_adv_set_fields(&adv_fields);
    if (rc != 0)
    {
        ESP_LOGE(TAG, "Failed setting adv data: %d", rc);
        return;
    }

    adv_params.conn_mode = BLE_GAP_CONN_MODE_NON;
    adv_params.disc_mode = BLE_GAP_DISC_MODE_GEN;

    uint32_t dur = (duration_ms == 0) ? BLE_HS_FOREVER : duration_ms;

    rc = ble_gap_adv_start(own_addr_type, NULL, dur, &adv_params,
                           ble_gap_event_handler, NULL);
    if (rc != 0)
    {
        ESP_LOGE(TAG, "Failed to start adv: %d", rc);
        return;
    }

    ESP_LOGI(TAG, "Advertising started (dur=%u ms, T=%.2f,H=%.2f,P=%.2f)",
             (unsigned)dur, g_temp, g_hum, g_press);
}

static void on_stack_reset(int reason)
{
    ESP_LOGW(TAG, "NimBLE stack reset: %d", reason);
}

static void on_stack_sync(void)
{
    ESP_LOGI(TAG, "NimBLE synced (no auto advertising)");
    ble_synced = true;
}

static void nimble_host_config_init(void)
{
    ble_hs_cfg.reset_cb = on_stack_reset;
    ble_hs_cfg.sync_cb = on_stack_sync;
    ble_hs_cfg.store_status_cb = ble_store_util_status_rr;

    ble_store_config_init();
}

static void nimble_host_task(void *param)
{
    ESP_LOGI(TAG, "NimBLE host task started");
    nimble_port_run();
    vTaskDelete(NULL);
}

void ble_init(void)
{
    esp_err_t ret;

    ret = nvs_flash_init();
    if (ret == ESP_ERR_NVS_NO_FREE_PAGES || ret == ESP_ERR_NVS_NEW_VERSION_FOUND)
    {
        ESP_ERROR_CHECK(nvs_flash_erase());
        nvs_flash_init();
    }

    nimble_port_init();

    ble_svc_gap_init();
    ble_svc_gap_device_name_set(DEVICE_NAME);

    ble_hs_id_infer_auto(0, &own_addr_type);
    ble_hs_id_copy_addr(own_addr_type, addr_val, NULL);

    nimble_host_config_init();

    xTaskCreate(nimble_host_task, "NimBLE Host", 4096, NULL, 5, NULL);
}

void ble_update_measurements(float temperature, float humidity, float pressure)
{
    g_temp = temperature;
    g_hum = humidity;
    g_press = pressure;
}
