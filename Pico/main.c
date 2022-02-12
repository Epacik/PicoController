/* 
    This code originates from the Getting started with Raspberry Pi Pico document
    https://datasheets.raspberrypi.org/pico/getting-started-with-pico.pdf
    CC BY-ND Raspberry Pi (Trading) Ltd
*/

#include <stdio.h>
#include "pico/stdlib.h"
#include "hardware/gpio.h"
#include "pico/binary_info.h"
#include "main.h"
#include "pins.h"


int main() {
    bi_decl(bi_program_description("PROJECT DESCRIPTION"));
    
    stdio_init_all();

    init_pins();

    bool button_held = false;

    while(true) {
        bool toggle_mute = gpio_get(Pins_ToggleMute);
        bool volume_up   = gpio_get(Pins_VolumeUp);
        bool volume_down = gpio_get(Pins_VolumeDown);
        if(toggle_mute && !button_held) {
            printf("Toggle Mute\n");
            button_held = true;
        }
        else if(volume_up /*&& !button_held*/) {
            printf("Volume Up\n");
            sleep_ms(200);
            //button_held = true;
        }
        else if (volume_down /*&& !button_held*/) {
            printf("Volume Down\n");
            sleep_ms(200);
            //button_held = true;
        }
        else if(!toggle_mute && !volume_up && !volume_down) {
            button_held = false;
        }
        sleep_ms(10);
    }
}

void init_pins(void) 
{
    init_pin(Pins_LED,        GPIO_OUT);
    init_pin(Pins_VolumeUp,   GPIO_IN);
    init_pin(Pins_VolumeDown, GPIO_IN);
    init_pin(Pins_ToggleMute, GPIO_IN);
}


void init_pin(int pin, int direction) 
{
    gpio_init(pin);
    gpio_set_dir(pin, direction);
}
