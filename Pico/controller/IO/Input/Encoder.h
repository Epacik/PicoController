#pragma once
#include "Input.h"

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
        Encoder(uint8_t id, IInputPin* pin0, IInputPin* pin1);
        IO::Input::Message* GetMessage() override;

    protected:
        Encoder(uint8_t id, InputType type);

        bool CompareStateAndPush(bool a, bool b, EncoderStates lastState, EncoderStates newState);
        bool CompareState(bool a, bool b, EncoderStates state);
        EncoderDirection CurrentDirection();
        void PushLastState(EncoderStates state);

    private:
        etl::array<EncoderStates, 4> lastStates;
    };
}