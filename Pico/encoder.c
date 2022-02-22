#include "encoder.h"

void encoder_states_push(EncoderStates *last_four_states, EncoderStates new_state);

EncoderDirection read_encoder_state(bool a, bool b, EncoderStates *last_four_states) 
{
    EncoderStates current_state = last_four_states[3];
    if(current_state != EncoderState_None && encoder_is_state(a, b, EncoderState_None)) {
        encoder_states_push(last_four_states, EncoderState_None);
    }
    else if(current_state != EncoderState_A && encoder_is_state(a, b, EncoderState_A)) {
        encoder_states_push(last_four_states, EncoderState_A);
    }
    else if(current_state != EncoderState_B && encoder_is_state(a, b, EncoderState_B)) {
        encoder_states_push(last_four_states, EncoderState_B);
    }
    else if(current_state != EncoderState_AB && encoder_is_state(a, b, EncoderState_AB)) {
        encoder_states_push(last_four_states, EncoderState_AB);
    }
    else {
       return EncoderDirection_None; 
    }

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

    if(last_four_states[0] == EncoderState_B &&
       last_four_states[1] == EncoderState_None && 
       last_four_states[2] == EncoderState_A &&
       last_four_states[3] == EncoderState_AB) {
           return EncoderDirection_CounterClockwise;
    }

    if(last_four_states[0] == EncoderState_A &&
       last_four_states[1] == EncoderState_None && 
       last_four_states[2] == EncoderState_B &&
       last_four_states[3] == EncoderState_AB) {
           return EncoderDirection_Clockwise;
    }

    return EncoderDirection_None;
}

bool encoder_is_state(bool a, bool b, EncoderStates test_state) {
    bool test_a = false;
    bool test_b = false;

    switch (test_state) {
        case EncoderState_A:
            test_a = true;
            break;
        case EncoderState_B:
            test_b = true;
            break;
        case EncoderState_AB:
            test_a = true;
            test_b = true;
            break;
    }

    if(a == test_a && b == test_b) {
        return true;
    }
    
    return false;
}

void encoder_states_push(EncoderStates *last_four_states, EncoderStates new_state) {
    last_four_states[0] = last_four_states[1];
    last_four_states[1] = last_four_states[2];
    last_four_states[2] = last_four_states[3];
    last_four_states[3] = new_state;
}

Encoder create_encoder(int id, Pins pin_a, Pins pin_b, Pins pin_button)
{
    EncoderStates last_states[] = {
        EncoderState_AB,
        EncoderState_AB,
        EncoderState_AB,
        EncoderState_AB
    };
    Encoder encoder = {
        id,
        pin_a, 
        pin_b, 
        pin_button,
        {
            EncoderState_AB,
            EncoderState_AB,
            EncoderState_AB,
            EncoderState_AB
        },
        false
    };
    return encoder;
}
