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
        const IO::Output::OutputType Type;

    protected:
        etl::vector<OutputPin *, 24> pins;

        explicit Output(OutputType type) : Type(type) {}
        virtual void Set(bool value) = 0;
    };
}