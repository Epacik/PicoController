#ifndef PICOCONTROLLERTESTS_PICOWDEFAULTLED_H
#define PICOCONTROLLERTESTS_PICOWDEFAULTLED_H
#include "LED.h"

namespace IO::Output {
    class PicoWDefaultLED : public LED {
    public:
        PicoWDefaultLED();
        void Set(bool value) override;
    };
}
#endif //PICOCONTROLLERTESTS_PICOWDEFAULTLED_H
