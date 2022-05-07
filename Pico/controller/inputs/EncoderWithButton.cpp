#include "EncoderWithButton.h"
namespace Inputs
{
    EncoderWithButton::EncoderWithButton(uint8_t id, uint32_t pin0, uint32_t pin1, uint32_t pinButton)
        : Encoder(id, InputType::EncoderWithButton)
    {
        this->pins = {pin0, pin1, pinButton};
    }

    Inputs::Message* EncoderWithButton::GetMessage()
    {
        auto newState = gpio_get(this->pins[2]);
        uint32_t buttonValue = 0;

        if (newState && !this->isHeld) // pressed
        {
            this->isHeld = true;
            buttonValue = 1;
        }
        else if (!newState && this->isHeld) // released
        {
            this->isHeld = false;
            buttonValue = 1 << 1;
        }

        uint32_t encoderValue = static_cast<uint32_t>(CurrentDirection());
        uint32_t value = (buttonValue << 2) | encoderValue;

        return value == 0 ? nullptr : CreateMessage(value);
    }
}