#include "Pins.h"
#include "hardware/gpio.h"
#include "etl/bitset.h"

void initPin(uint8_t pin, IO::PinDirection direction, IO::PinPull pull)
{

    gpio_init(pin);
    gpio_set_dir(pin, static_cast<bool>(direction));

    auto none = IO::PinPull::None;
    gpio_set_pulls(pin, (pull & IO::PinPull::Up) > none, (pull & IO::PinPull::Down) > none);
}

namespace IO {
    void InputPin::Init() 
    {
        initPin(pin, direction, pull);
    }

    bool InputPin::Read()
    {
        return gpio_get(pin);
    }

    void OutputPin::Init()
    {
        initPin(pin, direction, pull);
    }

    void OutputPin::Set(bool value)
    {
        gpio_put(pin, value);
    }
}