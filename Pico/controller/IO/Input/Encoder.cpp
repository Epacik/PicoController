#include "Encoder.h"
#include "mpark/patterns.hpp"

namespace IO::Input
{
    Encoder::Encoder(uint8_t id, uint8_t pin0, uint8_t pin1, bool softDebounce)
        : Encoder(
                id,
                new InputPin(pin0, PinPull::None, softDebounce),
                new InputPin(pin1,PinPull::None, softDebounce))
                {
        for (int i = 0; i < 4; ++i) {
            states[i] = EncoderStates::None;
        }
    }

    Encoder::Encoder(uint8_t id, InputType type = InputType::Encoder) : Input(id, type) {}
    
    Encoder::Encoder(uint8_t id, InputPin* pin0, InputPin* pin1) : Input(id, InputType::Encoder)
    {
        this->pins = { pin0, pin1 };
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
        printf("New State: A: %d; B: %d\r\n",
               HasState(currentState, EncoderStates::A),
               HasState(currentState, EncoderStates::B));

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
        if (state1 != EncoderStates::None || state3 != EncoderStates::AB)
            result = EncoderDirection::None;

        else if (state0 == EncoderStates::B && state2 == EncoderStates::A)
            result = EncoderDirection::CounterClockwise;

        else if (state0 == EncoderStates::A && state2 == EncoderStates::B)
            result = EncoderDirection::Clockwise;
        
        return result;
    }

    void Encoder::PushLastState(EncoderStates state)
    {
        states[0] = states[1];
        states[1] = states[2];
        states[2] = states[3];
        states[3] = state;
//        stateOffset = (stateOffset + 1) % 4;
//        states[stateOffset] = state;
    }

    EncoderStates Encoder::GetLastState(uint32_t i) {
        return states[(i + stateOffset) % 4];
    }

    EncoderStates Encoder::GetState() {
        using namespace mpark::patterns;
        auto a = this->pins[0]->Read();
        auto b = this->pins[1]->Read();

        return match(a, b)(
            pattern(true, false) = []() { return EncoderStates::A; },
            pattern(false, true) = []() { return EncoderStates::B; },
            pattern(true, true)  = []() { return EncoderStates::AB; },
            pattern(_, _)        = []() { return EncoderStates::None; }
        );
    }
}