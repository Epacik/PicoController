#pragma once
#include "Button.h"
#include "Encoder.h"
#include "EncoderWithButton.h"
#include "etl/memory.h"
#include "etl/memory_model.h"

namespace IO::Input
{
    static etl::vector<etl::unique_ptr<Input>, 256> CreateInputs()
    {
        etl::vector<etl::unique_ptr<Input>, 256> inputs;
        uint8_t id = 0;
        //Buttons
        inputs.push_back(etl::unique_ptr<Input>(new Button(id++, 17)));
        inputs.push_back(etl::unique_ptr<Input>(new Button(id++, 18)));
        inputs.push_back(etl::unique_ptr<Input>(new Button(id++, 19)));
        inputs.push_back(etl::unique_ptr<Input>(new Button(id++, 20)));

        //Encoders with buttons
        inputs.push_back(etl::unique_ptr<Input>(new EncoderWithButton(id++, 2, 3, 12)));
        inputs.push_back(etl::unique_ptr<Input>(new EncoderWithButton(id++, 4, 5, 13)));
        inputs.push_back(etl::unique_ptr<Input>(new EncoderWithButton(id++, 6, 7, 14)));
        inputs.push_back(etl::unique_ptr<Input>(new EncoderWithButton(id++, 8, 9, 15)));
        inputs.push_back(etl::unique_ptr<Input>(new EncoderWithButton(id++, 10, 11, 16)));

        return inputs;
    }
}