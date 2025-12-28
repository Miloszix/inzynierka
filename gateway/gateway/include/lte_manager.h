#pragma once
#include <stdbool.h>
#include "esp_err.h"

esp_err_t lte_manager_init(void);
bool lte_manager_is_connected(void);
