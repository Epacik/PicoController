#include "Button.h"

IO::Input::Button::Button(uint8_t id, uint8_t pin) :  Button(id, new InputPin(pin))
{}

IO::Input::Button::Button(uint8_t id, IInputPin* pin) : Input(id, InputType::Button)
{
    this->pins = { pin };
}

IO::Input::Message* IO::Input::Button::GetMessage()
{
    auto newState = this->pins[0]->Read();

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

