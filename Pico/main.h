#pragma once

#include <stdio.h>
#include "pico/stdlib.h"
#include "hardware/gpio.h"
#include "pico/binary_info.h"
#include "pins.h"
#include "encoder.h"

void init_pin(int pin, int direction, bool pull_up);
void init_pins(void);

