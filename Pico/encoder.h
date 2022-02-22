#pragma once
#include "pico/stdlib.h"
#include "pins.h"

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

struct Encoder {
    int id;
    Pins pin_a;
    Pins pin_b;
    Pins pin_button;
    EncoderStates last_states[4];
    bool button_held;
};
typedef struct Encoder Encoder;


EncoderDirection read_encoder_state(bool a, bool b, EncoderStates *last_four_states);
bool encoder_is_state(bool a, bool b, EncoderStates test_state);
Encoder create_encoder(int id, Pins pin_a, Pins pin_b, Pins pin_button);