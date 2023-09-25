#include <etl/array.h>
#include "Pins.h"
#include "hardware/gpio.h"
#include "../Time.h"

struct PinState {
    PinState()
    {
        State = false;
        UpdateUs = 0;
    }
    PinState(bool state, uint32_t time)
    {
        State = state;
        UpdateUs = time;
    }
    bool State;
    uint32_t UpdateUs;
};

etl::array<PinState, 32> _pinStates;
etl::array<bool, 32> _irqEnabled;
etl::array<bool, 32> _softDebounce;
bool callbackSet = false;

void gpioCallback(uint gpio, uint32_t events)
{
    auto us = Time::UsSinceBoot();
    auto currentState = _pinStates[gpio];

    if(_softDebounce[gpio] && us + 5 <= currentState.UpdateUs){
        return;
    }

    PinState state((events & GPIO_IRQ_EDGE_RISE) > 0, us);
    _pinStates[gpio] = state;
    //printf("Pin: %d, State: %d\r\n", gpio, state.State);
}

namespace IO {

    bool InputPin::Read()
    {
        //return _pinStates[pin].State;

        _debounce.add(gpio_get(pin));
        return  _debounce.is_held();
    }

    void OutputPin::Set(bool value)
    {
        gpio_put(pin, value);
    }

    void Pin::Init(bool softDebounce)
    {
        gpio_init(pin);
        gpio_set_dir(pin, static_cast<bool>(direction));
        _softDebounce[pin] = softDebounce;

        auto none = IO::PinPull::None;
        gpio_set_pulls(pin, (pull & IO::PinPull::Up) > none, (pull & IO::PinPull::Down) > none);

        if (direction == PinDirection::Input) {
            if(!_irqEnabled[pin]){
                gpio_set_irq_enabled(pin, GPIO_IRQ_EDGE_RISE | GPIO_IRQ_EDGE_FALL, true);
            }
            if (!callbackSet) {
                gpio_set_irq_callback(&gpioCallback);
                callbackSet = true;
            }
        }
    }
}