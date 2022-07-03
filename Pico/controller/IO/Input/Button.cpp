#include "Button.h"

enum class Values {
    Pressed = 1,
    Released = 1 << 1,
};

IO::Input::Button::Button(uint8_t id, uint8_t pin) :  Button(id, new InputPin(pin)) {}

IO::Input::Button::Button(uint8_t id, IInputPin* pin) : Input(id, InputType::Button)
{
    pins = { pin };
    isHeld = false;
}

IO::Input::Message* IO::Input::Button::GetMessage()
{
    auto newState = this->pins[0]->Read();

    if (newState && !this->isHeld) // pressed
    {
        this->isHeld = true;
        return CreateMessage(static_cast<uint32_t>(Values::Pressed));
    }

    if (!newState && this->isHeld) // released
    {
        this->isHeld = false;
        return CreateMessage(static_cast<uint32_t>(Values::Released));
    }

    return nullptr; // button didn't change state
}



