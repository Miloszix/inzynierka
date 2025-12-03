#pragma once
#include <stdbool.h>

void sensor_manager_init();
bool sensor_exists(const char *mac);
bool sensor_is_approved(const char *mac);
void sensor_add_pending(const char *mac);
void sensor_set_approved(const char *mac, bool approved);
