#pragma once
#include "Input.h"

namespace Inputs
{
    class Button : public Input
    {
    public:
        virtual Inputs::Message* GetMessage();
        Button(uint32_t id, uint32_t pin);

    private:
        bool isHeld;
    };
}