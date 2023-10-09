#pragma once

#include "Output.h"

namespace IO::Output{
    class LED : public Output {
    public:
        explicit LED(uint8_t pin);
        //LED(OutputPin* pin);
        void Set(bool) override;

        void Blink(uint32_t duration_ms);
        void Blink(uint32_t durationOn_ms, uint32_t durationOff_ms);
    };

    class EmptyLED : public LED {
    public:
        EmptyLED();
        void Set(bool) override;
    };
}

