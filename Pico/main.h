#pragma once

#include <stdio.h>
#include "pico/stdlib.h"
#include "hardware/gpio.h"
#include "pico/binary_info.h"
#include "pico/multicore.h"
#include "pins.h"
#include "Core1.h"
// #include "encoder.h"
#include "inputs/CreateInputs.h"


inline uint32_t get_us_since_boot(void);
void init_pin(Pins pin, int direction, bool pull_up);
void init_pins(void);

