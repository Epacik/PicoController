#pragma once
#include <cstdio>
#include "etl/vector.h"
#include "etl/array.h"
#include "etl/memory.h"
#include "hardware/gpio.h"
#include "../Pins.h"


namespace IO::Input
{
    enum class InputType : uint16_t
    {
        Button = 1,
        Encoder = 2,
        EncoderWithButton = 3,
        MIN [[maybe_unused]] = Button,
        MAX [[maybe_unused]] = EncoderWithButton,
    };

    struct Message
    {
        Message(uint8_t inputId, IO::Input::InputType inputType, uint32_t value)
            : InputId(inputId), InputType(inputType), Value(value) {}

        const uint8_t InputId;
        const IO::Input::InputType InputType;
        const uint32_t Value;
    };

    class Input
    {
    public:
        // return a notification to send to computer
        virtual IO::Input::Message* GetMessage() = 0;
        const uint8_t ID;
        const IO::Input::InputType Type;

    protected:
        etl::vector<IInputPin*, 24> pins;

        Input(uint8_t id, InputType type) : ID(id), Type(type) {}

        IO::Input::Message* CreateMessage(uint32_t value)
        {
            return new IO::Input::Message(this->ID, this->Type, value);
        }

    private:
    };

}