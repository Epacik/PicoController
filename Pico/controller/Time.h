#ifndef PICOCONTROLLERTESTS_TIME_H
#define PICOCONTROLLERTESTS_TIME_H


#include <cstdint>

namespace Time {
    uint32_t UsSinceBoot();
    uint32_t MsSinceBoot();
    uint32_t SecSinceBoot();
};


#endif //PICOCONTROLLERTESTS_TIME_H
