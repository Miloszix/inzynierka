#ifndef BLE_H
#define BLE_H

#include "esp_err.h"

esp_err_t ble_init(void);
void ble_update_measurements(float temperature, float humidity, float pressure);

#endif // BLE_H
