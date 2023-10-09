#pragma once

#include "LED.h"

namespace IO::Output {
    class PicoWDefaultLED : public LED {
    public:
        PicoWDefaultLED();
        void Set(bool value) override;
    };
}
