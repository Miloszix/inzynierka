#include <stdio.h>
#include "freertos/FreeRTOS.h"
#include "freertos/task.h"
#include "esp_log.h"
#include "driver/i2c.h"
#include "driver/uart.h"
#include "bme280.h"
#include "ble.h"

#define I2C_MASTER_SCL_IO 22
#define I2C_MASTER_SDA_IO 21

static float temperature, humidity, pressure;

void init_i2c() {
    i2c_config_t conf = {
        .mode = I2C_MODE_MASTER,
        .sda_io_num = I2C_MASTER_SDA_IO,
        .scl_io_num = I2C_MASTER_SCL_IO,
        .sda_pullup_en = GPIO_PULLUP_ENABLE,
        .scl_pullup_en = GPIO_PULLUP_ENABLE,
        .master.clk_speed = 100000
    };
    i2c_param_config(I2C_NUM_0, &conf);
    i2c_driver_install(I2C_NUM_0, conf.mode, 0, 0, 0);
}

void init_uart() {
    uart_config_t uart_config = {
        .baud_rate = 115200,
        .data_bits = UART_DATA_8_BITS,
        .parity = UART_PARITY_DISABLE,
        .stop_bits = UART_STOP_BITS_1,
        .flow_ctrl = UART_HW_FLOWCTRL_DISABLE
    };
    uart_driver_install(UART_NUM_0, 1024, 0, 0, NULL, 0);
    uart_param_config(UART_NUM_0, &uart_config);
}

void read_bme280_task(void *pvParameters) {
    while (1) {
        bme280_read_data(&temperature, &humidity, &pressure);
        ble_update_measurements(temperature, humidity, pressure);
        ESP_LOGI("BME280", "Temp: %.2f C, Hum: %.2f %%, Pres: %.2f hPa",
                 temperature, humidity, pressure);
        vTaskDelay(pdMS_TO_TICKS(2000));
    }
}

void app_main() {
    init_uart();
    init_i2c();
    bme280_init();
    ble_init();

    xTaskCreate(&read_bme280_task, "read_bme280_task", 4096, NULL, 5, NULL);
}
