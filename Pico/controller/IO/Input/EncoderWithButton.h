#pragma once
#include "Encoder.h"

namespace IO::Input
{
    class EncoderWithButton : public Encoder
    {
    public:
        EncoderWithButton(uint8_t id, uint8_t pin0, uint8_t pin1, uint8_t pinButton);
        EncoderWithButton(uint8_t id, IInputPin* pin0, IInputPin* pin1, IInputPin* pinButton);
        IO::Input::Message* GetMessage() override;

    private:
        bool isHeld{};
    };
}