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
        Encoder(uint8_t id, InputPin* pin0, InputPin* pin1, bool halfStep);
        IO::Input::Message* GetMessage() override;

    protected:
        Encoder(uint8_t id, InputType type, bool halfStep);

        EncoderDirection CurrentDirection();
        void PushLastState(EncoderStates state);
        EncoderStates GetLastState(uint32_t i);
        EncoderStates GetState();

    private:
        etl::array<EncoderStates, 4> _states;
        uint32_t _stateOffset = 0;
        bool _halfStep = false;
    };
}