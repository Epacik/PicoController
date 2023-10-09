#include "Button.h"

enum class Values {
    Pressed = 1,
    Released = 1 << 1,
};

IO::Input::Button::Button(uint8_t id, uint8_t pin, bool softDebounce) :  Button(id, new InputPin(pin,PinPull::None, softDebounce)) {}

IO::Input::Button::Button(uint8_t id, InputPin* pin) : Input(id, InputType::Button)
{
    pins = { pin };
    _isHeld = false;
}

IO::Input::Message* IO::Input::Button::GetMessage()
{
    auto newState = this->pins[0]->Read();

    if (newState && !this->_isHeld) // pressed
    {
        this->_isHeld = true;
        return CreateMessage(static_cast<uint32_t>(Values::Pressed));
    }

    if (!newState && this->_isHeld) // released
    {
        this->_isHeld = false;
        return CreateMessage(static_cast<uint32_t>(Values::Released));
    }

    return nullptr; // button didn't change state
}



