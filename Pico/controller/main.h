#pragma once

#include <cstdio>
#include "pico/stdlib.h"
#include "hardware/gpio.h"
#include "pico/binary_info.h"
#include "pico/multicore.h"
#include "Communication/Communication.h"
#include "UserInteractions.h"
#include "IO/CurrentIO.h"

[[noreturn]] [[noreturn]]
void HaltAndBlinkSos(etl::unique_ptr<IO::Output::LED> led);
