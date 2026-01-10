#include "mqtt_message.h"
#include "sensor_manager.h"
#include "lte_manager.h"
#include "config.h"
#include "esp_log.h"
#include "mqtt_client.h"
#include "freertos/FreeRTOS.h"
#include "freertos/task.h"
#include <cJSON.h>

static const char *TAG = "MQTT";
static esp_mqtt_client_handle_t mqtt_client = NULL;

static bool mqtt_connected = false;
static bool mqtt_started = false;

static esp_err_t mqtt_event_handler_cb(esp_mqtt_event_handle_t event)
{
    if (!event)
        return ESP_FAIL;

    switch (event->event_id)
    {
    case MQTT_EVENT_CONNECTED:
    {
        ESP_LOGI(TAG, "MQTT connected");
        mqtt_connected = true;

        char sub[128];
        snprintf(sub, sizeof(sub),
                 "server/%s/sensor/+/status", GATEWAY_ID);

        esp_mqtt_client_subscribe(event->client, sub, 1);
        ESP_LOGI(TAG, "Subscribed to %s", sub);

        char sub_sync[128];
        snprintf(sub_sync, sizeof(sub_sync),
                 "server/%s/sync_response", GATEWAY_ID);

        esp_mqtt_client_subscribe(event->client, sub_sync, 1);
        ESP_LOGI(TAG, "Subscribed to %s", sub_sync);

        mqtt_request_sync();
    }
    break;

    case MQTT_EVENT_DATA:
    {
        char topic[128] = {0};
        strncpy(topic, event->topic, event->topic_len);

        char payload[256] = {0};
        strncpy(payload, event->data, event->data_len);

        char status_prefix[64];
        snprintf(status_prefix, sizeof(status_prefix),
                 "server/%s/sensor/", GATEWAY_ID);

        if (strncmp(topic, status_prefix, strlen(status_prefix)) == 0 &&
            strstr(topic, "/status"))
        {
            // pull MAC from topic
            char mac[18] = {0};
            const char *p = topic + strlen(status_prefix);
            const char *slash = strstr(p, "/status");

            if (slash && (slash - p) < sizeof(mac))
            {
                strncpy(mac, p, slash - p);

                cJSON *root = cJSON_Parse(payload);
                if (root)
                {
                    const char *status =
                        cJSON_GetObjectItem(root, "status")->valuestring;

                    bool approved = strcmp(status, "accepted") == 0;

                    sensor_set_approved(mac, approved);

                    ESP_LOGI(TAG, "LIVE UPDATE: Sensor %s -> %s",
                             mac, approved ? "ACCEPTED" : "IGNORED");

                    cJSON_Delete(root);
                }
            }
            return ESP_OK;
        }

        char sync_topic[64];
        snprintf(sync_topic, sizeof(sync_topic),
                 "server/%s/sync_response", GATEWAY_ID);

        if (strcmp(topic, sync_topic) == 0)
        {
            ESP_LOGI(TAG, "Received sync response: %s", payload);

            cJSON *root = cJSON_Parse(payload);
            if (!root)
                break;

            cJSON *arr = cJSON_GetObjectItem(root, "sensors");
            if (cJSON_IsArray(arr))
            {
                int count = cJSON_GetArraySize(arr);
                for (int i = 0; i < count; i++)
                {
                    cJSON *item = cJSON_GetArrayItem(arr, i);
                    const char *mac = cJSON_GetObjectItem(item, "mac")->valuestring;
                    const char *status = cJSON_GetObjectItem(item, "status")->valuestring;

                    bool approved = strcmp(status, "accepted") == 0;

                    sensor_set_approved(mac, approved);

                    ESP_LOGI(TAG, "Sensor %s -> %s", mac, approved ? "ACCEPTED" : "IGNORED");
                }
            }

            cJSON_Delete(root);
        }
    }
    break;
    case MQTT_EVENT_DISCONNECTED:
        ESP_LOGW(TAG, "MQTT disconnected");
        mqtt_connected = false;
        break;
    case MQTT_EVENT_ERROR:
        ESP_LOGE(TAG, "MQTT error");
        mqtt_connected = false;
        break;

    default:
        break;
    }

    return ESP_OK;
}

static void mqtt_event_handler(void *handler_args, esp_event_base_t base,
                               int32_t event_id, void *event_data)
{
    mqtt_event_handler_cb(event_data);
}

void mqtt_notify_gateway_online()
{
    if (!mqtt_client)
        return;

    char topic[64];
    snprintf(topic, sizeof(topic),
             "gateway/%s/online", GATEWAY_ID);

    char payload[128];
    snprintf(payload, sizeof(payload),
             "{\"status\":\"online\",\"version\":\"1.0\"}");

    esp_mqtt_client_publish(mqtt_client, topic, payload, 0, 1, 0);

    ESP_LOGI(TAG, "Gateway online message sent: %s -> %s", topic, payload);
}

void mqtt_init(void)
{
    if (mqtt_started)
        return;

    ESP_LOGI(TAG, "Waiting for LTE connection...");

    int retry = 0;
    while (!lte_manager_is_connected() && retry < 120)
    {
        vTaskDelay(pdMS_TO_TICKS(500));
        retry++;
    }

    if (!lte_manager_is_connected())
    {
        ESP_LOGW(TAG, "LTE not connected, MQTT not started");
        return;
    }

    vTaskDelay(pdMS_TO_TICKS(3000));

    esp_mqtt_client_config_t cfg = {
        .broker.address.uri = "mqtt://3.70.126.6",
        .session.disable_clean_session = false,
        .credentials.client_id = GATEWAY_ID,
        .session.keepalive = 10,
        .network.timeout_ms = 10000,
        .network.reconnect_timeout_ms = 1000,
        .buffer.size = 1024,
    };

    mqtt_client = esp_mqtt_client_init(&cfg);
    esp_mqtt_client_register_event(
        mqtt_client,
        ESP_EVENT_ANY_ID,
        mqtt_event_handler,
        NULL);

    esp_mqtt_client_start(mqtt_client);

    mqtt_notify_gateway_online();
    mqtt_started = true;

    ESP_LOGI(TAG, "MQTT client started");
}

/* GATEWAY → SERVER (new sensor) */
void mqtt_send_new_sensor(const char *mac)
{
    if (!mqtt_client)
    {
        ESP_LOGW(TAG, "MQTT not initialized");
        return;
    }

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
    if (!mqtt_client)
    {
        ESP_LOGW(TAG, "MQTT not initialized");
        return;
    }

    char topic[128];
    snprintf(topic, sizeof(topic),
             "gateway/%s/sensor/%s/data", GATEWAY_ID, mac);

    char payload[160];
    snprintf(payload, sizeof(payload),
             "{\"temperature\":%.2f,\"humidity\":%.2f,\"pressure\":%.2f}",
             t, h, p);

    esp_mqtt_client_publish(mqtt_client, topic, payload, 0, 1, 0);
}

void mqtt_request_sync()
{
    if (!mqtt_client)
        return;

    char topic[64];
    snprintf(topic, sizeof(topic),
             "gateway/%s/request_sync", GATEWAY_ID);

    const char *payload = "{\"request\":\"sync\"}";

    esp_mqtt_client_publish(mqtt_client, topic, payload, 0, 1, 0);

    ESP_LOGI(TAG, "Sync request sent");
}
