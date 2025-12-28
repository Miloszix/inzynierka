#include "lte_manager.h"
#include "esp_log.h"
#include "esp_event.h"
#include "esp_netif.h"
#include "esp_netif_ppp.h"
#include "driver/uart.h"
#include "driver/gpio.h"
#include "esp_modem_api.h"
#include "freertos/FreeRTOS.h"
#include "freertos/task.h"
#include <string.h>

static const char *TAG = "LTE_MGR";

#define MODEM_UART UART_NUM_1
#define MODEM_TX 27
#define MODEM_RX 26
#define MODEM_PWRKEY 4
#define MODEM_BAUD 115200
#define MODEM_APN "TM"

static esp_netif_t *ppp_netif = NULL;
static esp_modem_dce_t *dce = NULL;
static bool g_ppp_ip_ready = false;

static void ip_event_handler(void *arg, esp_event_base_t base, int32_t id, void *data)
{
    if (id == IP_EVENT_PPP_GOT_IP)
    {
        ip_event_got_ip_t *event = (ip_event_got_ip_t *)data;
        ESP_LOGI(TAG, "Connected! IP: " IPSTR, IP2STR(&event->ip_info.ip));
        g_ppp_ip_ready = true;
    }
    else if (id == IP_EVENT_PPP_LOST_IP)
    {
        ESP_LOGW(TAG, "Connection lost");
        g_ppp_ip_ready = false;
    }
}

static void modem_hardware_reset(void)
{
    gpio_reset_pin(MODEM_PWRKEY);
    gpio_set_direction(MODEM_PWRKEY, GPIO_MODE_OUTPUT);
    ESP_LOGI(TAG, "Power cycling modem...");
    gpio_set_level(MODEM_PWRKEY, 0);
    vTaskDelay(pdMS_TO_TICKS(1200));
    gpio_set_level(MODEM_PWRKEY, 1);
    vTaskDelay(pdMS_TO_TICKS(5000));
}

esp_err_t lte_manager_init(void)
{
    g_ppp_ip_ready = false;

    static bool base_inited = false;
    if (!base_inited)
    {
        esp_netif_init();
        esp_event_loop_create_default();
        esp_event_handler_register(IP_EVENT, ESP_EVENT_ANY_ID, &ip_event_handler, NULL);
        base_inited = true;
    }

    if (!ppp_netif)
    {
        esp_netif_config_t cfg = ESP_NETIF_DEFAULT_PPP();
        ppp_netif = esp_netif_new(&cfg);
    }

    if (dce)
    {
        esp_modem_destroy(dce);
        dce = NULL;
    }

    modem_hardware_reset();

    esp_modem_dte_config_t dte_cfg = ESP_MODEM_DTE_DEFAULT_CONFIG();
    dte_cfg.uart_config.port_num = MODEM_UART;
    dte_cfg.uart_config.tx_io_num = MODEM_TX;
    dte_cfg.uart_config.rx_io_num = MODEM_RX;
    dte_cfg.uart_config.baud_rate = MODEM_BAUD;
    dte_cfg.uart_config.rx_buffer_size = 4096;

    esp_modem_dce_config_t dce_cfg = ESP_MODEM_DCE_DEFAULT_CONFIG(MODEM_APN);

    dce = esp_modem_new_dev(ESP_MODEM_DCE_SIM7000, &dte_cfg, &dce_cfg, ppp_netif);
    if (!dce)
        return ESP_FAIL;

    // Konfiguracja LTE-M i PDP
    esp_modem_at(dce, "AT+CFUN=1", NULL, 1000);
    esp_modem_at(dce, "AT+CIPSHUT", NULL, 1000);

    char pdp_cmd[64];
    snprintf(pdp_cmd, sizeof(pdp_cmd), "AT+CGDCONT=1,\"IP\",\"%s\"", MODEM_APN);
    esp_modem_at(dce, pdp_cmd, NULL, 1000);

    esp_modem_at(dce, "AT+CNMP=38", NULL, 1000); // LTE Only
    esp_modem_at(dce, "AT+CMNB=1", NULL, 1000);  // LTE-M

    ESP_LOGI(TAG, "Waiting for registration...");
    for (int i = 0; i < 20; i++)
    {
        char res[64];
        esp_modem_at(dce, "AT+CGREG?", res, 1000);
        if (strstr(res, "+CGREG: 0,1") || strstr(res, "+CGREG: 0,5"))
            break;
        vTaskDelay(pdMS_TO_TICKS(2000));
    }

    return esp_modem_set_mode(dce, ESP_MODEM_MODE_DATA);
}

bool lte_manager_is_connected(void)
{
    return g_ppp_ip_ready;
}