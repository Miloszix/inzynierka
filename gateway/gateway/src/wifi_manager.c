#include "wifi_manager.h"
#include "esp_wifi.h"
#include "esp_event.h"
#include "esp_log.h"
#include "nvs_flash.h"

static const char *TAG = "WIFI_MANAGER";
static bool g_wifi_connected = false;

/* ============================================
 *  USTAW SWOJE WIFI TUTAJ
 * ============================================ */
#define WIFI_SSID "KOREK810"
#define WIFI_PASSWORD "zmalqp10"

/* ============================================
 *   HANDLERY EVENTÓW
 * ============================================ */
static void wifi_event_handler(void *arg, esp_event_base_t base,
                               int32_t id, void *data)
{
    if (base == WIFI_EVENT)
    {
        switch (id)
        {
        case WIFI_EVENT_STA_START:
            esp_wifi_connect();
            break;

        case WIFI_EVENT_STA_DISCONNECTED:
            g_wifi_connected = false;
            ESP_LOGW(TAG, "WiFi lost → retry...");
            esp_wifi_connect();
            break;
        }
    }
    else if (base == IP_EVENT && id == IP_EVENT_STA_GOT_IP)
    {
        g_wifi_connected = true;

        ip_event_got_ip_t *event = (ip_event_got_ip_t *)data;
        ESP_LOGI(TAG, "Connected! IP: " IPSTR,
                 IP2STR(&event->ip_info.ip));
    }
}

/* ============================================
 *   START WiFi STA
 * ============================================ */
static void wifi_start_sta(void)
{
    wifi_config_t cfg = {0};
    strcpy((char *)cfg.sta.ssid, WIFI_SSID);
    strcpy((char *)cfg.sta.password, WIFI_PASSWORD);

    ESP_LOGI(TAG, "Connecting to SSID: %s", WIFI_SSID);

    esp_wifi_set_mode(WIFI_MODE_STA);
    esp_wifi_set_config(WIFI_IF_STA, &cfg);
    esp_wifi_start();
}

/* ============================================
 *   INIT
 * ============================================ */
void wifi_manager_init()
{
    // NVS wymagane przez WiFi
    nvs_flash_init();
    esp_netif_init();
    esp_event_loop_create_default();

    esp_netif_create_default_wifi_sta();

    wifi_init_config_t init_cfg = WIFI_INIT_CONFIG_DEFAULT();
    esp_wifi_init(&init_cfg);

    esp_event_handler_register(WIFI_EVENT, ESP_EVENT_ANY_ID, wifi_event_handler, NULL);
    esp_event_handler_register(IP_EVENT, IP_EVENT_STA_GOT_IP, wifi_event_handler, NULL);

    wifi_start_sta();
}

/* ============================================
 *   STATUS
 * ============================================ */
bool wifi_manager_is_connected()
{
    return g_wifi_connected;
}
