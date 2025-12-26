#include "lte_manager.h"
#include "esp_log.h"
#include "esp_event.h"
#include "esp_netif.h"
#include "driver/uart.h"
#include "driver/gpio.h"
#include "esp_modem_api.h"

static const char *TAG = "lte";

#define MODEM_UART UART_NUM_1
#define MODEM_TX 27
#define MODEM_RX 26
#define MODEM_PWRKEY 4
#define MODEM_BAUD 115200
#define MODEM_APN "TM"

static esp_netif_t *ppp_netif = NULL;
static esp_modem_dce_t *dce = NULL;
static bool g_lte_connected = false;

static void ip_event_handler(void *arg, esp_event_base_t event_base, int32_t event_id, void *event_data)
{
    if (event_id == IP_EVENT_PPP_GOT_IP)
    {
        ip_event_got_ip_t *event = (ip_event_got_ip_t *)event_data;
        ESP_LOGI(TAG, "PPP GOT IP: " IPSTR, IP2STR(&event->ip_info.ip));
        g_lte_connected = true;
    }
    else if (event_id == IP_EVENT_PPP_LOST_IP)
    {
        ESP_LOGW(TAG, "PPP LOST");
        g_lte_connected = false;
    }
}

static void modem_power_on(void)
{
    gpio_set_direction(MODEM_PWRKEY, GPIO_MODE_OUTPUT);
    ESP_LOGI(TAG, "Powering modem ON (Pulse)...");
    gpio_set_level(MODEM_PWRKEY, 0);
    vTaskDelay(pdMS_TO_TICKS(1200));
    gpio_set_level(MODEM_PWRKEY, 1);
    vTaskDelay(pdMS_TO_TICKS(5000));
}

void lte_manager_init(void)
{
    modem_power_on();

    // Inicjalizacja stosu sieciowego
    esp_err_t err = esp_netif_init();
    if (err != ESP_OK && err != ESP_ERR_INVALID_STATE)
        ESP_ERROR_CHECK(err);
    err = esp_event_loop_create_default();
    if (err != ESP_OK && err != ESP_ERR_INVALID_STATE)
        ESP_ERROR_CHECK(err);

    ESP_ERROR_CHECK(esp_event_handler_register(IP_EVENT, ESP_EVENT_ANY_ID, &ip_event_handler, NULL));

    esp_netif_config_t netif_cfg = ESP_NETIF_DEFAULT_PPP();
    ppp_netif = esp_netif_new(&netif_cfg);

    esp_modem_dte_config_t dte_cfg = ESP_MODEM_DTE_DEFAULT_CONFIG();
    dte_cfg.uart_config.port_num = MODEM_UART;
    dte_cfg.uart_config.tx_io_num = MODEM_TX;
    dte_cfg.uart_config.rx_io_num = MODEM_RX;
    dte_cfg.uart_config.baud_rate = MODEM_BAUD;

    esp_modem_dce_config_t dce_cfg = ESP_MODEM_DCE_DEFAULT_CONFIG(MODEM_APN);

    dce = esp_modem_new_dev(ESP_MODEM_DCE_SIM7000, &dte_cfg, &dce_cfg, ppp_netif);
    assert(dce);

    // Konfiguracja trybu LTE-M
    ESP_LOGI(TAG, "Configuring modem for LTE-M...");
    esp_modem_at(dce, "AT+CFUN=1", NULL, 1000);
    esp_modem_at(dce, "AT+CNMP=38", NULL, 1000); // LTE Only
    esp_modem_at(dce, "AT+CMNB=1", NULL, 1000);  // Cat-M1

    // --- INTELIGENTNE OCZEKIWANIE NA REJESTRACJĘ ---
    ESP_LOGI(TAG, "Waiting for network registration...");
    bool registered = false;
    char response[64];

    // Zwiększamy limit do 60 prób (łącznie ok. 2 minuty), ale wychodzimy od razu po sukcesie
    for (int i = 0; i < 60; i++)
    {
        memset(response, 0, sizeof(response));
        // Pobieramy surową odpowiedź z modemu
        esp_modem_at(dce, "AT+CGREG?", response, 1000);

        // Szukamy statusu 1 (home) lub 5 (roaming)
        if (strstr(response, "+CGREG: 0,1") || strstr(response, "+CGREG: 0,5") ||
            strstr(response, "+CGREG: 1,1") || strstr(response, "+CGREG: 1,5"))
        {
            ESP_LOGI(TAG, "Registered successfully after %d seconds!", i * 2);
            registered = true;
            break;
        }

        ESP_LOGI(TAG, "Still searching... (%d/60)", i + 1);
        vTaskDelay(pdMS_TO_TICKS(2000)); // Sprawdzaj co 2 sekundy
    }

    if (!registered)
    {
        ESP_LOGE(TAG, "Failed to register in network (Timeout). Check antenna or APN.");
        return;
    }

    // Dodatkowe 1s stabilizacji przed wejściem w PPP
    vTaskDelay(pdMS_TO_TICKS(1000));

    ESP_LOGI(TAG, "Setting mode to DATA (PPP)...");
    err = esp_modem_set_mode(dce, ESP_MODEM_MODE_DATA);
    if (err != ESP_OK)
    {
        ESP_LOGE(TAG, "PPP mode failed: %s", esp_err_to_name(err));
    }
}

bool lte_manager_is_connected(void)
{
    return g_lte_connected;
}