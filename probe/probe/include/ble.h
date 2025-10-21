#ifndef BLE_H
#define BLE_H

#include "esp_err.h"

#include "services/gap/ble_svc_gap.h"

#define BLE_GAP_APPEARANCE_GENERIC_TAG 0x0200
#define BLE_GAP_URI_PREFIX_HTTPS 0x17
#define BLE_GAP_LE_ROLE_PERIPHERAL 0x00

#define TAG "SENSOR"
#define DEVICE_NAME "SENSOR"

void ble_init(void);
void ble_update_measurements(float temperature, float humidity, float pressure);

void adv_init(void);
int gap_init(void);


#endif // BLE_H
