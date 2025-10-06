#ifndef BME280_H
#define BME280_H

#include "esp_err.h"
#include <stdint.h>

#ifdef __cplusplus
extern "C" {
#endif

typedef int32_t BME280_S32_t;
typedef uint32_t BME280_U32_t;
typedef int64_t BME280_S64_t;

/**
 * @brief Inicjalizuje czujnik BME280.
 * 
 * Ustawia tryb pracy, oversampling i odczytuje dane kalibracyjne.
 *
 * @return ESP_OK jeśli sukces, inaczej kod błędu.
 */
esp_err_t bme280_init(void);

/**
 * @brief Odczytuje dane z czujnika BME280.
 *
 * Funkcja odczytuje i kompensuje dane temperatury, wilgotności i ciśnienia.
 *
 * @param temperature [out] Wskaźnik na zmienną do przechowania temperatury (°C)
 * @param humidity [out] Wskaźnik na zmienną do przechowania wilgotności (%)
 * @param pressure [out] Wskaźnik na zmienną do przechowania ciśnienia (hPa)
 * @return ESP_OK jeśli sukces, inaczej kod błędu.
 */
esp_err_t bme280_read_data(float *temperature, float *humidity, float *pressure);

#ifdef __cplusplus
}
#endif

#endif // BME280_H
