#include "wifi_manager.h"
#include "sensor_manager.h"
#include "mqtt_message.h"
#include "ble_scanner.h"
#include "lte_manager.h"

void app_main()
{
    sensor_manager_init(); // 1. tablica sensorów
    lte_manager_init();    // 2. LTE-M (PPPoS)
    mqtt_init();           // 3. MQTT (czeka na LTE)
    ble_init();            // 4. BLE scanner
}
