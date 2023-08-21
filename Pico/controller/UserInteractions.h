#include "IO/Output/LED.h"

#include <cstdio>
#include "pico/stdlib.h"
#include "etl/memory.h"

class UserInteractions {
public:
    [[noreturn]] static void Entry();
    static void Initialize();
    [[maybe_unused]] static void SOS(IO::Output::LED* led);

};