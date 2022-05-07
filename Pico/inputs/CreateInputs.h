#pragma once
#include "Button.h"
#include "EncoderWithButton.h"
#include "../pins.h"

namespace Inputs
{
    static std::vector<std::unique_ptr<Input>> CreatedInputs()
    {
        std::vector<std::unique_ptr<Input>> inputs;
        uint32_t id = 0;

        inputs.push_back(std::make_unique<Button>(id++, 17));
        inputs.push_back(std::make_unique<Button>(id++, 18));
        inputs.push_back(std::make_unique<Button>(id++, 19));
        inputs.push_back(std::make_unique<Button>(id++, 20));

        inputs.push_back(std::make_unique<EncoderWithButton>(id++, 2, 3, 12));
        inputs.push_back(std::make_unique<EncoderWithButton>(id++, 4, 5, 13));
        inputs.push_back(std::make_unique<EncoderWithButton>(id++, 6, 7, 14));
        inputs.push_back(std::make_unique<EncoderWithButton>(id++, 8, 9, 15));
        inputs.push_back(std::make_unique<EncoderWithButton>(id++, 10, 11, 16));

        return inputs;
    }
}