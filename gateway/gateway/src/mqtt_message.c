#include "mqtt_message.h"
#include "sensor_manager.h"
#include "esp_log.h"
#include "mqtt_client.h"
#include <cJSON.h>

static const char *TAG = "MQTT";
static esp_mqtt_client_handle_t mqtt_client = NULL;

static const char *GATEWAY_ID = "GW001";

/* EVENT HANDLER (inner) */
static esp_err_t mqtt_event_handler_cb(esp_mqtt_event_handle_t event)
{
    if (!event) return ESP_FAIL;

    switch (event->event_id)
    {
        case MQTT_EVENT_CONNECTED: {
            ESP_LOGI(TAG, "MQTT connected");

            char sub[128];
            snprintf(sub, sizeof(sub),
                     "server/%s/sensor/+/status", GATEWAY_ID);

            esp_mqtt_client_subscribe(event->client, sub, 1);
            ESP_LOGI(TAG, "Subscribed to %s", sub);
        }
        break;

        case MQTT_EVENT_DATA:
        {
            char topic[128] = {0};
            strncpy(topic, event->topic, event->topic_len);

            char payload[256] = {0};
            strncpy(payload, event->data, event->data_len);

            ESP_LOGI(TAG, "Topic: %s", topic);
            ESP_LOGI(TAG, "Payload: %s", payload);

            // Expected: server/GW001/sensor/<MAC>/status
            char prefix[64];
            snprintf(prefix, sizeof(prefix),
                     "server/%s/sensor/", GATEWAY_ID);

            if (strncmp(topic, prefix, strlen(prefix)) == 0) {

                const char *rest = topic + strlen(prefix);
                const char *mac_end = strstr(rest, "/status");
                if (!mac_end) break;

                char mac[32] = {0};
                int len = mac_end - rest;
                strncpy(mac, rest, len);

                cJSON *root = cJSON_Parse(payload);
                if (!root) break;

                cJSON *status = cJSON_GetObjectItem(root, "status");
                if (cJSON_IsString(status)) {

                    if (strcmp(status->valuestring, "accepted") == 0) {
                        sensor_set_approved(mac, true);
                        ESP_LOGI(TAG, "Sensor %s ACCEPTED", mac);
                    }
                    else if (strcmp(status->valuestring, "ignored") == 0) {
                        sensor_set_approved(mac, false);
                        ESP_LOGI(TAG, "Sensor %s IGNORED", mac);
                    }
                }
                cJSON_Delete(root);
            }
        }
        break;

        default:
            break;
    }

    return ESP_OK;
}

/* EVENT WRAPPER */
static void mqtt_event_handler(void *handler_args, esp_event_base_t base,
                               int32_t event_id, void *event_data)
{
    mqtt_event_handler_cb(event_data);
}

void mqtt_notify_gateway_online()
{
    if (!mqtt_client) return;

    char topic[64];
    snprintf(topic, sizeof(topic),
             "gateway/%s/online", GATEWAY_ID);

    char payload[128];
    snprintf(payload, sizeof(payload),
             "{\"status\":\"online\",\"version\":\"1.0\"}");

    esp_mqtt_client_publish(mqtt_client, topic, payload, 0, 1, 0);

    ESP_LOGI(TAG, "Gateway online message sent: %s -> %s", topic, payload);
}

/* INIT MQTT */
void mqtt_init(void)
{
    if (mqtt_client) return;

    esp_mqtt_client_config_t cfg = {
        .broker.address.uri = "mqtt://3.70.126.6",
    };

    mqtt_client = esp_mqtt_client_init(&cfg);
    esp_mqtt_client_register_event(mqtt_client,
                                   ESP_EVENT_ANY_ID,
                                   mqtt_event_handler,
                                   NULL);
    esp_mqtt_client_start(mqtt_client);

    mqtt_notify_gateway_online();
}

/* GATEWAY → SERVER (new sensor) */
void mqtt_send_new_sensor(const char *mac)
{
    char topic[128];
    snprintf(topic, sizeof(topic),
             "gateway/%s/sensor/new", GATEWAY_ID);

    char payload[128];
    snprintf(payload, sizeof(payload),
             "{\"sensor_mac\":\"%s\"}", mac);

    esp_mqtt_client_publish(mqtt_client, topic, payload, 0, 1, 0);
}

/* GATEWAY → SERVER (data) */
void mqtt_send_sensor_data(const char *mac, float t, float h, float p)
{
    char topic[128];
    snprintf(topic, sizeof(topic),
             "gateway/%s/sensor/%s/data", GATEWAY_ID, mac);

    char payload[160];
    snprintf(payload, sizeof(payload),
             "{\"temperature\":%.2f,\"humidity\":%.2f,\"pressure\":%.2f}",
             t, h, p);

    esp_mqtt_client_publish(mqtt_client, topic, payload, 0, 1, 0);
}

