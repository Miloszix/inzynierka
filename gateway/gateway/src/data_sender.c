#include "data_sender.h"
#include "sensor_manager.h"
#include "mqtt_message.h"
#include "esp_log.h"

static const char *TAG = "DATA_SENDER";

void data_sender_handle_sensor(
    const char *mac,
    float t, float h, float p)
{
    if (!sensor_exists(mac))
    {
        ESP_LOGI(TAG, "New sensor detected: %s", mac);
        sensor_add_pending(mac);
        mqtt_send_new_sensor(mac);
        return;
    }

    if (!sensor_is_approved(mac))
    {
        ESP_LOGI(TAG, "Sensor %s not approved yet", mac);
        return;
    }

    mqtt_send_sensor_data(mac, t, h, p);
}
