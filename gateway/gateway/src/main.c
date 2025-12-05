#include "wifi_manager.h"
#include "sensor_manager.h"
#include "mqtt_message.h"
#include "ble_scanner.h"

void app_main()
{
    wifi_manager_init();   // 1. WiFi
    sensor_manager_init(); // 2. Sensory table
    mqtt_init();           // 3. MQTT
    ble_init();            // 4. BLE scanner
}
