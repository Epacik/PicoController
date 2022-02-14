#include "main.h"

int main() {
    bi_decl(bi_program_description("PROGRAM USED TO CONTROL VOLUME IN WINDOWS"));
    stdio_init_all();

    init_pins();

    gpio_put(Pins_LED, true);
    sleep_ms(50);
    gpio_put(Pins_LED, false);

    bool button_held = false;
    EncoderStates last_states[] = {
        EncoderState_AB,
        EncoderState_AB,
        EncoderState_AB,
        EncoderState_AB
    };
    
    while(true) {
        bool toggle_mute = gpio_get(Pins_ToggleMute);
        bool volume_up   = gpio_get(Pins_EncoderA);
        bool volume_down = gpio_get(Pins_EncoderB);

        if(toggle_mute && !button_held) {
            printf("Toggle Mute\n");
            button_held = true;
        }
        else if(!toggle_mute) {
            button_held = false;
        }
        
        EncoderDirection direction = read_encoder_state(volume_up, volume_down, last_states);
        
        if(direction == EncoderDirection_Clockwise) {
            printf("Volume Up\n");
            //sleep_ms(10);
        }
        else if (direction == EncoderDirection_CounterClockwise) {
            printf("Volume Down\n");
            //sleep_ms(10);
        }
        sleep_ms(1);
    }
}

void init_pins(void) 
{
    init_pin(Pins_LED,        GPIO_OUT, false);
    init_pin(Pins_EncoderA,   GPIO_IN,  true);
    init_pin(Pins_EncoderB,   GPIO_IN,  true);
    init_pin(Pins_ToggleMute, GPIO_IN,  false);
}


void init_pin(int pin, int direction, bool pull_up) 
{
    gpio_init(pin);
    gpio_set_dir(pin, direction);
    if(direction == GPIO_IN) {
        gpio_set_pulls(pin, pull_up, !pull_up);
    }
}
