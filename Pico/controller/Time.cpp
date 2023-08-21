#include <pico/types.h>
#include <pico/time.h>
#include "Time.h"

uint32_t Time::UsSinceBoot() {
    return to_us_since_boot(get_absolute_time());
}

uint32_t Time::MsSinceBoot() {
    return to_ms_since_boot(get_absolute_time());
}

uint32_t Time::SecSinceBoot() {
    return to_us_since_boot(get_absolute_time()) / 1000;
}
