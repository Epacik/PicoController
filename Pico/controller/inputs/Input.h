#pragma once
#include <stdio.h>
#include "etl/vector.h"
#include "etl/array.h"
#include "etl/memory.h"
#include "hardware/gpio.h"

namespace Inputs
{
    enum class InputType : uint16_t
    {
        Button = 1,
        Encoder = 2,
        EncoderWithButton = 3,
    };

    struct Message
    {
        Message(uint8_t inputId, Inputs::InputType inputType, uint32_t value)
            : InputId(inputId), InputType(inputType), Value(value) {}

        const uint8_t InputId;
        const Inputs::InputType InputType;
        const uint32_t Value;
    };

    class Input
    {
    public:
        // return a notification to send to computer
        virtual Inputs::Message* GetMessage() = 0;
        const uint8_t ID;
        const InputType Type;

    protected:
        etl::vector<uint32_t, 24> pins;

        Input(uint32_t id, InputType type) : ID(id), Type(type) {}

        Inputs::Message* CreateMessage(uint32_t value)
        {
            return new Inputs::Message(this->ID, this->Type, value);
        }

    private:
    };

}