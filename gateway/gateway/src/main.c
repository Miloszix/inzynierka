#include "wifi_manager.h"
#include "sensor_manager.h"
#include "mqtt_message.h"
#include "ble_scanner.h"
#include "lte_manager.h"
#include "nvs_flash.h"
#include "esp_log.h"

void app_main()
{
    esp_err_t ret = nvs_flash_init();
    if (ret == ESP_ERR_NVS_NO_FREE_PAGES || ret == ESP_ERR_NVS_NEW_VERSION_FOUND)
    {
        ESP_ERROR_CHECK(nvs_flash_erase());
        ret = nvs_flash_init();
    }
    ESP_ERROR_CHECK(ret);
    sensor_manager_init(); // 1. tablica sensorów
    for (int i = 0; i < 3; i++)
    {
        ESP_LOGI("APP_MAIN", "Starting in %d...", 3 - i);
        if (lte_manager_init() == ESP_OK)
        {
            ESP_LOGI("APP_MAIN", "LTE initialized successfully");
            break;
        }
        else
        {
            ESP_LOGW("APP_MAIN", "LTE init failed, retrying... (%d/3)", i + 1);
        }
    }
    mqtt_init(); // 3. MQTT (czeka na LTE)
    ble_init();  // 4. BLE scanner
}
