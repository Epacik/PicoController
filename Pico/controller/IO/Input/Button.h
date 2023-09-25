#pragma once
#include "Input.h"

namespace IO::Input
{
    class Button : public Input
    {
    public:
        IO::Input::Message* GetMessage() override;
        Button(uint8_t id, uint8_t pin, bool softDebounce);
        Button(uint8_t id, InputPin* pin);

    private:
        bool isHeld;
    };
}