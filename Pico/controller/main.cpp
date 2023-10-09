#include "etl_profile.h"
#include "main.h"

#ifdef BOARD_PICO_W
    #include <pico/cyw43_arch.h>
#endif



int main()
{
    bi_decl(bi_program_description("PROGRAM USED TO CONTROL VOLUME IN WINDOWS... OR MORE"));
    stdio_init_all();

    auto hasBattery = IO::Battery::HasBattery();
    if(hasBattery){
        IO::Battery::Initialize();
    }

    Communication::Initialize(hasBattery);
    multicore_launch_core1(Communication::Entry);
    UserInteractions::Initialize();
    UserInteractions::Entry();
}
