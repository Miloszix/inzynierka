#pragma once

void mqtt_init();
void mqtt_send_new_sensor(const char *mac);
void mqtt_send_sensor_data(const char *mac, float t, float h, float p);
void mqtt_request_sync();
