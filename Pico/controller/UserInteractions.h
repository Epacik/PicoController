#include "IO/Output/LED.h"

#include <cstdio>
#include "pico/stdlib.h"

class UserInteractions {
public:
    [[noreturn]] static void Entry();
    static void Initialize();

private:
    [[maybe_unused]] static void SOS(IO::Output::LED* led);
    static inline uint32_t UsSinceBoot();
};