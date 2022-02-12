#pragma once

struct PinDirection {
    int pin;
    int direction;
};
typedef struct PinDirection PinDirection;

enum Pins {
    Pins_VolumeUp   = 11U,
    Pins_VolumeDown = 12U,
    Pins_ToggleMute = 13U,

    Pins_LED        = 25U,
};
typedef enum Pins Pins;

