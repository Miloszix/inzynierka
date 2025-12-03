#include "sensor_manager.h"
#include <string.h>

typedef struct {
    char mac[18];
    bool approved;
} sensor_state_t;

static sensor_state_t sensors[32];
static int sensor_count = 0;

void sensor_manager_init() {
    sensor_count = 0;
}

bool sensor_exists(const char *mac) {
    for (int i = 0; i < sensor_count; i++) {
        if (strcmp(sensors[i].mac, mac) == 0)
            return true;
    }
    return false;
}

bool sensor_is_approved(const char *mac) {
    for (int i = 0; i < sensor_count; i++) {
        if (strcmp(sensors[i].mac, mac) == 0)
            return sensors[i].approved;
    }
    return false;
}

void sensor_add_pending(const char *mac) {
    strcpy(sensors[sensor_count].mac, mac);
    sensors[sensor_count].approved = false;
    sensor_count++;
}

void sensor_set_approved(const char *mac, bool approved) {
    for (int i = 0; i < sensor_count; i++) {
        if (strcmp(sensors[i].mac, mac) == 0) {
            sensors[i].approved = approved;
            break;
        }
    }
}
