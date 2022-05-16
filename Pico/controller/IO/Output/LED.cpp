#include <pico/time.h>
#include "LED.h"



void IO::Output::LED::Set(bool value)
{
    pins[0]->Set(value);
}

IO::Output::LED::LED(uint8_t id, IO::IOutputPin *pin) : Output(id, OutputType::LED)
{
    this->pins = {pin};
}

IO::Output::LED::LED(uint8_t id, uint8_t pin) : LED(id, new IO::OutputPin(pin)) {}

void IO::Output::LED::Blink(uint32_t duration_ms)
{
    Set(true);
    sleep_ms(duration_ms);
    Set(false);
}

void IO::Output::LED::Blink(uint32_t durationOn_ms, uint32_t durationOff_ms)
{
    Blink(durationOn_ms);
    sleep_ms(durationOff_ms);
}



