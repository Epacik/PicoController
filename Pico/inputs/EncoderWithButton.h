#pragma once
#include "Encoder.h"

namespace Inputs
{
    class EncoderWithButton : public Encoder
    {
    public:
        EncoderWithButton(uint32_t id, uint32_t pin0, uint32_t pin1, uint32_t pinButton);
        virtual Inputs::Message* GetMessage();

    private:
        bool isHeld;
    };
}