//
// Created by epat on 14.05.2022.
//

#ifndef VOLUMECONTROL_IO_OUTPUT_LED_H
#define VOLUMECONTROL_IO_OUTPUT_LED_H

#include "Output.h"

namespace IO::Output{
    class LED : Output {
    public:
        LED(uint8_t id, uint8_t pin);
        LED(uint8_t id, IOutputPin* pin);
        void Set(bool value);

        void Blink(uint32_t duration_ms);
        void Blink(uint32_t durationOn_ms, uint32_t durationOff_ms);
    };
}



#endif //VOLUMECONTROL_IO_OUTPUT_LED_H
