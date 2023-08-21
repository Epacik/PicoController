#include <pico/time.h>
#include "LED.h"



void IO::Output::LED::Set(bool value)
{
    pins[0]->Set(value);
}

IO::Output::LED::LED(uint8_t pin) : Output(OutputType::LED) {
    if (pin > 0) {
        this->pins = { new IO::OutputPin(pin) };
    }

}

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


IO::Output::EmptyLED::EmptyLED() : LED(0) {

}

void IO::Output::EmptyLED::Set(bool) {

}
