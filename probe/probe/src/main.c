#include <stdbool.h>
#include <stdio.h>
#include <string.h>

/* ESP APIs */
#include "esp_log.h"
#include "sdkconfig.h"
#include "esp_err.h"
#include "driver/i2c.h"
#include "driver/uart.h"
#include "esp_sleep.h"
#include "host/ble_gap.h"

/* FreeRTOS APIs */
#include <freertos/FreeRTOS.h>
#include <freertos/task.h>

#include "bme280.h"
#include "ble.h"

#define I2C_MASTER_SCL_IO 22
#define I2C_MASTER_SDA_IO 21

static float temperature, humidity, pressure;

void init_i2c()
{
    i2c_config_t conf = {
        .mode = I2C_MODE_MASTER,
        .sda_io_num = I2C_MASTER_SDA_IO,
        .scl_io_num = I2C_MASTER_SCL_IO,
        .sda_pullup_en = GPIO_PULLUP_ENABLE,
        .scl_pullup_en = GPIO_PULLUP_ENABLE,
        .master.clk_speed = 100000};
    i2c_param_config(I2C_NUM_0, &conf);
    i2c_driver_install(I2C_NUM_0, conf.mode, 0, 0, 0);
}

void init_uart()
{
    uart_config_t uart_config = {
        .baud_rate = 115200,
        .data_bits = UART_DATA_8_BITS,
        .parity = UART_PARITY_DISABLE,
        .stop_bits = UART_STOP_BITS_1,
        .flow_ctrl = UART_HW_FLOWCTRL_DISABLE};
    uart_driver_install(UART_NUM_0, 1024, 0, 0, NULL, 0);
    uart_param_config(UART_NUM_0, &uart_config);
}

void sensor_ble_task(void *pvParameters)
{
    const int active_ms = 5000; // advert time
    const int cycle_ms = 60000; // full cycle time

    // Poczekaj, aż NimBLE się zsynchronizuje
    while (!ble_synced)
    {
        ESP_LOGI("MAIN", "Waiting for NimBLE to sync...");
        vTaskDelay(pdMS_TO_TICKS(100));
    }

    while (1)
    {
        // 1. Read
        bme280_read_data(&temperature, &humidity, &pressure);
        ESP_LOGI("BME280", "Temp: %.2f C, Hum: %.2f %%, Pres: %.2f hPa",
                 temperature, humidity, pressure);

        // Update data
        ble_update_measurements(temperature, humidity, pressure);

        // START advertising for a specified time — stack stops it automatically
        start_advertising(active_ms);
        ESP_LOGI("BLE", "Advertising scheduled for %d ms", active_ms);

        // Wait for active_ms + small margin (advertising will be stopped by the stack and ADV_COMPLETE event)
        vTaskDelay(pdMS_TO_TICKS(active_ms + 50));

        // Sleep for the rest of the cycle
        int sleep_time = cycle_ms - active_ms;
        // Sleep
        ESP_LOGI("BLE", "Sleeping for %d ms", sleep_time);

        // set timer wakeup:
        esp_sleep_enable_timer_wakeup((uint64_t)sleep_time * 1000);

        // enter light sleep
        esp_light_sleep_start();

        // after waking up, code returns HERE
        ESP_LOGI("BLE", "Woke up!");
    }
}

void app_main()
{

    init_uart();
    init_i2c();
    bme280_init();
    ble_init();

    xTaskCreate(sensor_ble_task, "sensor_ble_task", 4096, NULL, 5, NULL);
}
