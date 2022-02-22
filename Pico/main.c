#include "main.h"

int main() {
    bi_decl(bi_program_description("PROGRAM USED TO CONTROL VOLUME IN WINDOWS"));
    stdio_init_all();

    init_pins();

    gpio_put(Pins_LED, true);
    sleep_ms(50);
    gpio_put(Pins_LED, false);

    bool button_held = false;

    Encoder encoders[] = {
        create_encoder(0, Pins_Encoder_0A, Pins_Encoder_0B, Pins_Encoder_0Button),
        create_encoder(1, Pins_Encoder_1A, Pins_Encoder_1B, Pins_Encoder_1Button),
        create_encoder(2, Pins_Encoder_2A, Pins_Encoder_2B, Pins_Encoder_2Button),
        create_encoder(3, Pins_Encoder_3A, Pins_Encoder_3B, Pins_Encoder_3Button),
        create_encoder(4, Pins_Encoder_4A, Pins_Encoder_4B, Pins_Encoder_4Button),
    };
    
    while(true) {

        for (int i = 0; i < 5; i++) {
            Encoder encoder = encoders[i];

            bool toggle_mute = gpio_get(encoder.pin_button);
            bool encoder_a   = gpio_get(encoder.pin_a);
            bool encoder_b   = gpio_get(encoder.pin_b);

            if(toggle_mute && !encoder.button_held) {
                printf("Encoder %d: Button pressed\n", encoder.id);
                encoder.button_held = true;
            }
            else if(!toggle_mute && encoder.button_held) {
                printf("Encoder %d: Button released\n", encoder.id);
                encoder.button_held = false;
            }

            EncoderDirection direction = read_encoder_state(encoder_a, encoder_b, encoder.last_states);
        
            if(direction == EncoderDirection_Clockwise) {
                printf("Encoder %d: Clockwise\n", encoder.id);
            }
            else if (direction == EncoderDirection_CounterClockwise) {
                printf("Encoder %d: Counter clockwise\n", encoder.id);
            }

            encoders[i] = encoder;
            
        }

        sleep_us(500);
        //sleep_ms(1);
    }
}

void init_pins(void) 
{
    init_pin(Pins_LED,             GPIO_OUT, false);

    init_pin(Pins_Encoder_0A,      GPIO_IN,  true);
    init_pin(Pins_Encoder_0B,      GPIO_IN,  true);
    init_pin(Pins_Encoder_0Button, GPIO_IN,  false);

    init_pin(Pins_Encoder_1A,      GPIO_IN,  true);
    init_pin(Pins_Encoder_1B,      GPIO_IN,  true);
    init_pin(Pins_Encoder_1Button, GPIO_IN,  false);

    init_pin(Pins_Encoder_2A,      GPIO_IN,  true);
    init_pin(Pins_Encoder_2B,      GPIO_IN,  true);
    init_pin(Pins_Encoder_2Button, GPIO_IN,  false);

    init_pin(Pins_Encoder_3A,      GPIO_IN,  true);
    init_pin(Pins_Encoder_3B,      GPIO_IN,  true);
    init_pin(Pins_Encoder_3Button, GPIO_IN,  false);

    init_pin(Pins_Encoder_4A,      GPIO_IN,  true);
    init_pin(Pins_Encoder_4B,      GPIO_IN,  true);
    init_pin(Pins_Encoder_4Button, GPIO_IN,  false);
}


void init_pin(int pin, int direction, bool pull_up) 
{
    gpio_init(pin);
    gpio_set_dir(pin, direction);
    if(direction == GPIO_IN) {
        gpio_set_pulls(pin, pull_up, !pull_up);
    }
}
