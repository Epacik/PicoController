#pragma once
#include "pico/stdlib.h"

enum EncoderStates {
    EncoderState_None = 0,
    EncoderState_A    = 1,
    EncoderState_B    = 1 << 1,
    EncoderState_AB   = EncoderState_A | EncoderState_B,
};
typedef enum EncoderStates EncoderStates;

enum EncoderDirection {
    EncoderDirection_None              = 0,
    EncoderDirection_CounterClockwise  = 1,
    EncoderDirection_Clockwise         = 2,
};
typedef enum EncoderDirection EncoderDirection;

EncoderDirection read_encoder_state(bool a, bool b, EncoderStates *last_four_states);
bool encoder_is_state(bool a, bool b, EncoderStates test_state);