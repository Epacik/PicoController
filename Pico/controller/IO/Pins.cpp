#include <etl/array.h>
#include "Pins.h"
#include "hardware/gpio.h"
#include "../Time.h"

#define var auto
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

etl::array<IO::Pin*, 32> _pins;
etl::array<PinState, 32> _pinStates;
etl::array<bool, 32> _irqEnabled;
etl::array<bool, 32> _softDebounce;
bool callbackSet = false;

void gpioCallback(uint gpio, uint32_t events)
{
    auto us = Time::UsSinceBoot();
    auto currentState = _pinStates[gpio];

    auto debunceTime = _softDebounce[gpio] ? 50 : 5;

    if(us + debunceTime <= currentState.UpdateUs){
        return;
    }

    PinState state((events & GPIO_IRQ_EDGE_RISE) > 0, us);
    _pinStates[gpio] = state;
    //_pins[gpio]->AddState((events & GPIO_IRQ_EDGE_RISE) > 0);
    //printf("Pin: %d, State: %d\r\n", gpio, state.State);
}

namespace IO {

    bool InputPin::Read()
    {
        var debounce = _softDebounce[pin];
        //return _pinStates[pin].State;
        auto state = gpio_get(pin);

        _debounce.add(state);
        return _debounce.is_set();
    }

    void InputPin::AddState(bool b) {
        Pin::AddState(b);
        _debounce.add(b);
    }

    void InputPin::Init(bool softDebounce) {
        Pin::Init(softDebounce);

        if (softDebounce) {
            _debounce.set(1000, 0);
        }
        else {
            _debounce.set(1, 0);
        }
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
        _pins[pin] = this;
    }

    void Pin::AddState(bool b) {

    }
}