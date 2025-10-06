#include "bme280.h"
#include "driver/i2c.h"
#include "esp_log.h"
#include <string.h>

#define I2C_MASTER_NUM I2C_NUM_0
#define BME280_ADDR 0x76
#define TAG "BME280"

static uint16_t dig_T1;
static int16_t dig_T2, dig_T3;
static uint16_t dig_P1;
static int16_t dig_P2, dig_P3, dig_P4, dig_P5, dig_P6, dig_P7, dig_P8, dig_P9;
static uint8_t dig_H1, dig_H3;
static int16_t dig_H2, dig_H4, dig_H5, dig_H6;
static BME280_S32_t t_fine;

static esp_err_t write_register(uint8_t reg, uint8_t value) {
    i2c_cmd_handle_t cmd = i2c_cmd_link_create();
    i2c_master_start(cmd);
    i2c_master_write_byte(cmd, (BME280_ADDR << 1) | I2C_MASTER_WRITE, true);
    i2c_master_write_byte(cmd, reg, true);
    i2c_master_write_byte(cmd, value, true);
    i2c_master_stop(cmd);
    esp_err_t ret = i2c_master_cmd_begin(I2C_MASTER_NUM, cmd, pdMS_TO_TICKS(100));
    i2c_cmd_link_delete(cmd);
    return ret;
}

static esp_err_t read_register(uint8_t reg, uint8_t *value) {
    i2c_cmd_handle_t cmd = i2c_cmd_link_create();
    i2c_master_start(cmd);
    i2c_master_write_byte(cmd, (BME280_ADDR << 1) | I2C_MASTER_WRITE, true);
    i2c_master_write_byte(cmd, reg, true);
    i2c_master_start(cmd);
    i2c_master_write_byte(cmd, (BME280_ADDR << 1) | I2C_MASTER_READ, true);
    i2c_master_read_byte(cmd, value, I2C_MASTER_LAST_NACK);
    i2c_master_stop(cmd);
    esp_err_t ret = i2c_master_cmd_begin(I2C_MASTER_NUM, cmd, pdMS_TO_TICKS(100));
    i2c_cmd_link_delete(cmd);
    return ret;
}

static esp_err_t read_block(uint8_t reg, uint8_t *buf, size_t len) {
    i2c_cmd_handle_t cmd = i2c_cmd_link_create();
    i2c_master_start(cmd);
    i2c_master_write_byte(cmd, (BME280_ADDR << 1) | I2C_MASTER_WRITE, true);
    i2c_master_write_byte(cmd, reg, true);
    i2c_master_start(cmd);
    i2c_master_write_byte(cmd, (BME280_ADDR << 1) | I2C_MASTER_READ, true);
    if (len > 1)
        i2c_master_read(cmd, buf, len - 1, I2C_MASTER_ACK);
    i2c_master_read_byte(cmd, buf + len - 1, I2C_MASTER_LAST_NACK);
    i2c_master_stop(cmd);
    esp_err_t ret = i2c_master_cmd_begin(I2C_MASTER_NUM, cmd, pdMS_TO_TICKS(200));
    i2c_cmd_link_delete(cmd);
    return ret;
}

void configure_mode(uint8_t mode) {
    uint8_t regValue;
    read_register(0xF4, &regValue);         
    regValue &= ~(0x03);                           
    regValue |= (mode & 0x03);                     
    write_register(0xF4, regValue);                
}

static void read_calibration_data() {
    uint8_t calib[32];
    read_block(0x88, calib, 26);

    dig_T1 = (calib[1] << 8) | calib[0];
    dig_T2 = (calib[3] << 8) | calib[2];
    dig_T3 = (calib[5] << 8) | calib[4];
    dig_P1 = (calib[7] << 8) | calib[6];
    dig_P2 = (calib[9] << 8) | calib[8];
    dig_P3 = (calib[11] << 8) | calib[10];
    dig_P4 = (calib[13] << 8) | calib[12];
    dig_P5 = (calib[15] << 8) | calib[14];
    dig_P6 = (calib[17] << 8) | calib[16];
    dig_P7 = (calib[19] << 8) | calib[18];
    dig_P8 = (calib[21] << 8) | calib[20];
    dig_P9 = (calib[23] << 8) | calib[22];
    dig_H1 = calib[25];

    read_block(0xE1, calib, 7);
    dig_H2 = (calib[1] << 8) | calib[0];
    dig_H3 = calib[2];
    dig_H4 = (calib[3] << 4) | (calib[4] & 0x0F);
    dig_H5 = (calib[5] << 4) | (calib[4] >> 4);
    dig_H6 = (int8_t)calib[6];
}

BME280_S32_t BME280_compensate_T_int32(BME280_S32_t adc_T) { 
    BME280_S32_t var1, var2, T;

    // Temperature compensation algorithm
    var1 = ((((adc_T >> 3) - ((BME280_S32_t)dig_T1 << 1))) * ((BME280_S32_t)dig_T2)) >> 11;
    var2 = (((((adc_T >> 4) - ((BME280_S32_t)dig_T1)) * ((adc_T >> 4) - ((BME280_S32_t)dig_T1))) >> 12) *
            ((BME280_S32_t)dig_T3)) >> 14;
    t_fine = var1 + var2;        // Fine temperature value for compensation
    T = (t_fine * 5 + 128) >> 8; // Final compensated temperature value
    return T; 
}
BME280_U32_t BME280_compensate_P_int64(BME280_S32_t adc_P) { BME280_S64_t var1, var2, p;

    // Pressure compensation algorithm
    var1 = ((BME280_S64_t)t_fine) - 128000;
    var2 = var1 * var1 * (BME280_S64_t)dig_P6;
    var2 = var2 + ((var1 * (BME280_S64_t)dig_P5) << 17);
    var2 = var2 + (((BME280_S64_t)dig_P4) << 35);
    var1 = ((var1 * var1 * (BME280_S64_t)dig_P3) >> 8) + ((var1 * (BME280_S64_t)dig_P2) << 12);
    var1 = (((((BME280_S64_t)1) << 47) + var1)) * ((BME280_S64_t)dig_P1) >> 33;

    if (var1 == 0)
    {
        return 0; // Avoid division by zero error
    }

    p = 1048576 - adc_P;
    p = (((p << 31) - var2) * 3125) / var1;
    var1 = (((BME280_S64_t)dig_P9) * (p >> 13) * (p >> 13)) >> 25;
    var2 = (((BME280_S64_t)dig_P8) * p) >> 19;
    p = ((p + var1 + var2) >> 8) + (((BME280_S64_t)dig_P7) << 4);
    return (BME280_U32_t)p;
 }
BME280_U32_t BME280_compensate_H_int32(BME280_S32_t adc_H) { 
    BME280_S32_t v_x1_u32r;

    // Humidity compensation algorithm
    v_x1_u32r = (t_fine - ((BME280_S32_t)76800));
    v_x1_u32r = (((((adc_H << 14) - (((BME280_S32_t)dig_H4) << 20) - (((BME280_S32_t)dig_H5) * v_x1_u32r)) +
                ((BME280_S32_t)16384)) >> 15) * (((((((v_x1_u32r * ((BME280_S32_t)dig_H6)) >> 10) * (((v_x1_u32r *
                ((BME280_S32_t)dig_H3)) >> 11) + ((BME280_S32_t)32768))) >> 10) + ((BME280_S32_t)2097152)) *
                ((BME280_S32_t)dig_H2) +8192) >> 14));

    // Apply the limits to the humidity value
    v_x1_u32r = (v_x1_u32r - (((((v_x1_u32r >> 15) * (v_x1_u32r >> 15)) >> 7) * ((BME280_S32_t)dig_H1)) >> 4));
    v_x1_u32r = (v_x1_u32r < 0 ? 0 : v_x1_u32r);
    v_x1_u32r = (v_x1_u32r > 419430400 ? 419430400 : v_x1_u32r);
    return (BME280_U32_t)(v_x1_u32r >> 12); 
}

esp_err_t bme280_init() {
    uint8_t id = 0;
    ESP_ERROR_CHECK(read_register(0xD0, &id));
    if (id != 0x60) {
        ESP_LOGE(TAG, "BME280 not found (ID: 0x%02X)", id);
        return ESP_FAIL;
    }

    ESP_LOGI(TAG, "BME280 detected");
    write_register(0xF2, 0b101); // Humidity oversampling x16
    write_register(0xF4, 0b01101101); // Normal mode, temp/press oversampling x16
    write_register(0xF5, 0b10101100); // Standby 1000ms, filter coeff 16

    read_calibration_data();
    return ESP_OK;
}

// Compensation functions go here...
// (can be added in a second step)

esp_err_t bme280_read_data(float *temperature, float *humidity, float *pressure) {
    uint8_t data[8];

    configure_mode(0x01);

    esp_err_t ret = read_block(0xF7, data, 8);
    if (ret != ESP_OK) return ret;

    int32_t adc_P = (data[0] << 12) | (data[1] << 4) | (data[2] >> 4);
    int32_t adc_T = (data[3] << 12) | (data[4] << 4) | (data[5] >> 4);
    int32_t adc_H = (data[6] << 8) | data[7];

    // Compensation logic to be implemented here...

    *temperature = BME280_compensate_T_int32(adc_T) / 100.0;
    *pressure = BME280_compensate_P_int64(adc_P) / 256.0 / 100.0;
    *humidity = BME280_compensate_H_int32(adc_H) / 1024.0;

    return ESP_OK;
}
