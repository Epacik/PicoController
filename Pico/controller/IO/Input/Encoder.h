#pragma once
#include "Input.h"
#include "etl/circular_buffer.h"

namespace IO::Input
{
    enum class EncoderStates
    {
        None = 0,
        A = 1,
        B = 1 << 1,
        AB = A | B,
    };

    enum class EncoderDirection
    {
        None = 0,
        CounterClockwise = 1,
        Clockwise = 1 << 1,
    };

    class Encoder : public Input
    {
    public:
        Encoder(uint8_t id, uint8_t pin0, uint8_t pin1);
        Encoder(uint8_t id, InputPin* pin0, InputPin* pin1);
        IO::Input::Message* GetMessage() override;

    protected:
        Encoder(uint8_t id, InputType type);

        EncoderDirection CurrentDirection();
        void PushLastState(EncoderStates state);
        EncoderStates GetLastState(uint32_t i);
        EncoderStates GetState();

    private:
        //etl::array<EncoderStates, 4> lastStates;
        etl::array<EncoderStates, 4> states;
        uint32_t stateOffset = 0;
    };
}