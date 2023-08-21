#pragma once
#include "Input/Input.h"
#include "Output/Output.h"
#include "Output/LED.h"
#include "etl/memory.h"
#include "etl/memory_model.h"

namespace IO
{
    etl::vector<etl::unique_ptr<IO::Input::Input>, 256> GetInputs();
    etl::vector<etl::unique_ptr<IO::Output::Output>, 256> GetOutputs();
    etl::unique_ptr<IO::Output::LED> GetDefaultLED();

    namespace Battery {
        void Initialize();
        float GetBatteryVoltage();
        bool HasBattery();
    }
}