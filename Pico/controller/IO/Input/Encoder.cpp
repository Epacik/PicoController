#include "Encoder.h"

namespace IO::Input
{
    Encoder::Encoder(uint8_t id, uint8_t pin0, uint8_t pin1) : Encoder(id, new InputPin(id), new InputPin(id)) {}

    Encoder::Encoder(uint8_t id, InputType type = InputType::Encoder) : Input(id, type) {}
    
    Encoder::Encoder(uint8_t id, IInputPin* pin0, IInputPin* pin1) : Input(id, InputType::Encoder)
    {
        this->pins = {pin0, pin1};
    }

    IO::Input::Message* Encoder::GetMessage()
    {
        auto direction = CurrentDirection();
        switch (direction)
        {
        case EncoderDirection::None:
            return nullptr;
        default:
            return CreateMessage(static_cast<uint32_t>(direction));
        }
    }

    bool Encoder::CompareState(bool a, bool b, EncoderStates state)
    {
        switch (state)
        {
        case EncoderStates::A:
            return a && !b;
        case EncoderStates::B:
            return !a && b;
        case EncoderStates::AB:
            return a && b;
        case EncoderStates::None:
            return !a && !b;
        }
        return false;
    }

    bool Encoder::CompareStateAndPush(bool a, bool b, EncoderStates lastState, EncoderStates newState)
    {
        if (lastState != newState && CompareState(a, b, newState))
        {
            PushLastState(newState);
            return true;
        }

        return false;
    }

    EncoderDirection Encoder::CurrentDirection()
    {
        auto newA = this->pins[0]->Read();
        auto newB = this->pins[1]->Read();

        auto lastState = lastStates.back();

        // I only want to push one of them at the time, so that seems aproprieate
        if (CompareStateAndPush(newA, newB, lastState, EncoderStates::None)) {}
        else if (CompareStateAndPush(newA, newB, lastState, EncoderStates::A)) {}
        else if (CompareStateAndPush(newA, newB, lastState, EncoderStates::B)) {}
        else if (CompareStateAndPush(newA, newB, lastState, EncoderStates::AB)) {}
        else
            return EncoderDirection::None;

        /*
        counter clockwise
        A: 0, B: 1
        A: 0, B: 0
        A: 1, B: 0
        A: 1, B: 1

        clockwise
        A: 1, B: 0
        A: 0, B: 0
        A: 0, B: 1
        A: 1, B: 1
        */

        auto result = EncoderDirection::None;
        if (lastStates[1] != EncoderStates::None || lastStates[3] != EncoderStates::AB)
            result = EncoderDirection::None;

        else if (lastStates[0] == EncoderStates::B && lastStates[2] == EncoderStates::A)
            result = EncoderDirection::CounterClockwise;

        else if (lastStates[0] == EncoderStates::A && lastStates[2] == EncoderStates::B)
            result = EncoderDirection::Clockwise;
        
        return result;
    }

    void Encoder::PushLastState(EncoderStates state)
    {
        lastStates[0] = lastStates[1];
        lastStates[1] = lastStates[2];
        lastStates[2] = lastStates[3];
        lastStates[3] = state;
    }

}