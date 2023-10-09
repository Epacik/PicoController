#include "EncoderWithButton.h"

enum class ButtonValues {
    None     = 0,
    Pressed  = 1 << 2,
    Released = 1 << 3,
};

namespace IO::Input
{
    using namespace IO;
    
    EncoderWithButton::EncoderWithButton(uint8_t id, InputPin* pin0, InputPin* pin1, InputPin* pinButton, bool halfStep)
        : Encoder(id, InputType::EncoderWithButton, halfStep)
    {
        pins = {pin0, pin1, pinButton};
        isHeld = false;
    }

    IO::Input::Message* EncoderWithButton::GetMessage()
    {
        auto newState = this->pins[2]->Read();
        ButtonValues buttonValue = ButtonValues::None;

        if (newState && !this->isHeld) // pressed
        {
            this->isHeld = true;
            buttonValue = ButtonValues::Pressed;
        }
        else if (!newState && this->isHeld) // released
        {
            this->isHeld = false;
            buttonValue = ButtonValues::Released;
        }

        uint32_t value = static_cast<uint32_t>(buttonValue) | static_cast<uint32_t>(CurrentDirection());

        return value == 0 ? nullptr : CreateMessage(value);
    }
}