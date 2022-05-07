#include "etl_profile.h"
#include "main.h"

int main() {
    bi_decl(bi_program_description("PROGRAM USED TO CONTROL VOLUME IN WINDOWS... OR MORE"));
    stdio_init_all();

    multicore_launch_core1(Core1::Main);
    init_pins();

    gpio_put(static_cast<uint>(Pins::BuildtInLED), true);
    sleep_ms(50);
    gpio_put(static_cast<uint>(Pins::BuildtInLED), false);

    bool button_held = false;

    auto inputs = Inputs::CreatedInputs();
    
    uint32_t time = get_us_since_boot();

    while(true) {
        // wait until at least next half of a ms to help with debounce buttons a bit
        // technically it should work without it, 
        // but there's a possibility that I have chosen wrong resistors or soldered something wrong
        if(time + 500 >= get_us_since_boot()) {
            continue;
        }

        for (auto &input : inputs) {
            auto message = input->GetMessage();
            if(message == nullptr)
                continue;
            //send to other core
            Core1::PushOntoMessageQueue(message);
        }

        time = get_us_since_boot();
    }
}

void init_pins() 
{
    init_pin(Pins::BuildtInLED,     GPIO_OUT, false);

    init_pin(Pins::Encoder_0A,      GPIO_IN,  true);
    init_pin(Pins::Encoder_0B,      GPIO_IN,  true);
    init_pin(Pins::Encoder_0Button, GPIO_IN,  false);

    init_pin(Pins::Encoder_1A,      GPIO_IN,  true);
    init_pin(Pins::Encoder_1B,      GPIO_IN,  true);
    init_pin(Pins::Encoder_1Button, GPIO_IN,  false);

    init_pin(Pins::Encoder_2A,      GPIO_IN,  true);
    init_pin(Pins::Encoder_2B,      GPIO_IN,  true);
    init_pin(Pins::Encoder_2Button, GPIO_IN,  false);

    init_pin(Pins::Encoder_3A,      GPIO_IN,  true);
    init_pin(Pins::Encoder_3B,      GPIO_IN,  true);
    init_pin(Pins::Encoder_3Button, GPIO_IN,  false);

    init_pin(Pins::Encoder_4A,      GPIO_IN,  true);
    init_pin(Pins::Encoder_4B,      GPIO_IN,  true);
    init_pin(Pins::Encoder_4Button, GPIO_IN,  false);
}


void init_pin(Pins pin, int direction, bool pull_up) 
{
    auto iPin = static_cast<uint>(pin);
    gpio_init(iPin);
    gpio_set_dir(iPin, direction);
    if(direction == GPIO_IN) {
        gpio_set_pulls(iPin, pull_up, !pull_up);
    }
}

uint32_t get_us_since_boot(void)
{
    return to_us_since_boot(get_absolute_time());
}
