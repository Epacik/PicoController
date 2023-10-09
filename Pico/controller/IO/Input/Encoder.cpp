#include "Encoder.h"
#include "mpark/patterns.hpp"

namespace IO::Input
{
    Encoder::Encoder(uint8_t id, InputType type = InputType::Encoder, bool halfStep = false) : Input(id, type)
    {
        _halfStep = halfStep;
    }
    
    Encoder::Encoder(uint8_t id, InputPin* pin0, InputPin* pin1, bool halfStep) : Input(id, InputType::Encoder)
    {
        this->pins = { pin0, pin1 };
        _halfStep = halfStep;
    }

    bool HasState(EncoderStates value, EncoderStates state)
    {
        auto iValue = static_cast<uint32_t>(value);
        auto iState = static_cast<uint32_t>(state);
        return (iValue & iState) > 0;
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


    EncoderDirection Encoder::CurrentDirection()
    {
        auto currentState = GetState();
        auto lastState = GetLastState(3);

        if (lastState == currentState)
            return EncoderDirection::None;

        PushLastState(currentState);

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

        auto state0 = GetLastState(0);
        auto state1 = GetLastState(1);
        auto state2 = GetLastState(2);
        auto state3 = GetLastState(3);

        auto result = EncoderDirection::None;
        if (this->_halfStep){
            if ((state3 == EncoderStates::AB && state2 == EncoderStates::A) ||
                (state3 == EncoderStates::None && state2 == EncoderStates::B))
                result = EncoderDirection::CounterClockwise;

            if ((state3 == EncoderStates::AB && state2 == EncoderStates::B) ||
                (state3 == EncoderStates::None && state2 == EncoderStates::A))
                result = EncoderDirection::Clockwise;
        }
        else {
            if (state1 != EncoderStates::None || state3 != EncoderStates::AB)
                result = EncoderDirection::None;

            else if (state0 == EncoderStates::B && state2 == EncoderStates::A)
                result = EncoderDirection::CounterClockwise;

            else if (state0 == EncoderStates::A && state2 == EncoderStates::B)
                result = EncoderDirection::Clockwise;
        }
        
        return result;
    }

    void Encoder::PushLastState(EncoderStates state)
    {
        _states[0] = _states[1];
        _states[1] = _states[2];
        _states[2] = _states[3];
        _states[3] = state;
//        _stateOffset = (_stateOffset + 1) % 4;
//        _states[_stateOffset] = state;
    }

    EncoderStates Encoder::GetLastState(uint32_t i) {
        return _states[(i + _stateOffset) % 4];
    }

    EncoderStates Encoder::GetState() {
        using namespace mpark::patterns;
        auto a = this->pins[0]->Read();
        auto b = this->pins[1]->Read();

//        if(this->ID == 7){
//            printf("New State: A: %d; B: %d\r\n", a, b);
//        }
        return match(a, b)(
            pattern(true, false) = []() { return EncoderStates::A; },
            pattern(false, true) = []() { return EncoderStates::B; },
            pattern(true, true)  = []() { return EncoderStates::AB; },
            pattern(_, _)        = []() { return EncoderStates::None; }
        );
    }
}