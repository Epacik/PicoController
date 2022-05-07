#include "Button.h"

Inputs::Button::Button(uint8_t id, uint32_t pin) : Input(id, InputType::Button)
{
    this->pins = {pin};
}

Inputs::Message* Inputs::Button::GetMessage()
{
    auto newState = gpio_get(this->pins[0]);

    if (newState && !this->isHeld) // pressed
    {
        this->isHeld = true;
        return CreateMessage(1);
    }

    if (!newState && this->isHeld) // released
    {
        this->isHeld = false;
        return CreateMessage(1 << 1);
    }

    return nullptr; // button didn't change state
}