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

#ifdef BOARD_PICO_W
    if (cyw43_arch_init()) {
        printf("bluetooth init failed");
        return -1;
    }
    if (hasBattery) {
        (void)IO::Battery::GetBatteryVoltage();

        bool value = true;
        auto led = IO::GetDefaultLED();
        for (int i = 0; i < 6; i++) {
            led->Set(value);
            sleep_ms(1000);
            value = !value;
        }

        if (IO::Battery::GetBatteryVoltage() < 1.85) {
            HaltAndBlinkSos(etl::move(led));
        }

    }
#endif

    Communication::Initialize(hasBattery);
    multicore_launch_core1(Communication::Entry);
    UserInteractions::Initialize();
    UserInteractions::Entry();
}


void HaltAndBlinkSos(etl::unique_ptr<IO::Output::LED> led)
{
    auto ptr = led.get();
    while(true)
    {
        UserInteractions::SOS(ptr);
        sleep_ms(2000);
    }
}
