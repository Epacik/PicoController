#pragma once
#include "Input.h"
#include "Button.h"
#include "Encoder.h"
#include "EncoderWithButton.h"
#include "etl/memory.h"
#include "etl/memory_model.h"

namespace IO::Input
{
    etl::vector<etl::unique_ptr<Input>, 256> CreateInputs();
}