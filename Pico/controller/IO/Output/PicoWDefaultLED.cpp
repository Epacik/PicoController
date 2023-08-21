#include "PicoWDefaultLED.h"
#ifdef BOARD_PICO_W
#include "pico/cyw43_arch.h"
#endif

IO::Output::PicoWDefaultLED::PicoWDefaultLED() : LED (0) {

}

void IO::Output::PicoWDefaultLED::Set(bool value) {
#ifdef BOARD_PICO_W
    cyw43_arch_gpio_put(CYW43_WL_GPIO_LED_PIN, value);
#endif
}
