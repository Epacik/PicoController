#pragma once

#include <cstdio>
#include "etl/vector.h"
#include "etl/array.h"
#include "etl/memory.h"
#include "hardware/gpio.h"
#include "../Pins.h"

namespace IO::Output
{

    enum class OutputType : uint16_t
    {
        LED = 1,
        MIN [[maybe_unused]] = LED,
        MAX [[maybe_unused]] = LED,
    };

    class Output
    {

    public:
        const uint8_t ID;
        const IO::Output::OutputType Type;

    protected:
        etl::vector<IOutputPin *, 24> pins;

        Output(uint8_t id, OutputType type) : ID(id), Type(type) {}
    };
}