#pragma once
#include "Encoder.h"

namespace IO::Input
{
    class EncoderWithButton : public Encoder
    {
    public:
        EncoderWithButton(uint8_t id, InputPin* pin0, InputPin* pin1, InputPin* pinButton, bool halfStep);
        IO::Input::Message* GetMessage() override;

    private:
        bool isHeld{};
    };
}