#include "EncoderWithButton.h"
namespace IO::Input
{
    using namespace IO;
    EncoderWithButton::EncoderWithButton(uint8_t id, uint8_t pin0, uint8_t pin1, uint8_t pinButton) 
        : EncoderWithButton(id, new InputPin(pin0), new InputPin(pin1), new InputPin(pinButton))
    {}
    
    EncoderWithButton::EncoderWithButton(uint8_t id, IInputPin* pin0, IInputPin* pin1, IInputPin* pinButton)
        : Encoder(id, InputType::EncoderWithButton)
    {
        this->pins = {pin0, pin1, pinButton};
    }

    IO::Input::Message* EncoderWithButton::GetMessage()
    {
        auto newState = this->pins[2]->Read();
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