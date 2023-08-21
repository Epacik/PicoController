#include "UserInteractions.h"
#include "IO/CurrentIO.h"
#include "IO/Output/LED.h"
#include "Communication/Communication.h"
#include "Time.h"

etl::vector<etl::unique_ptr<IO::Input::Input>, 256> inputs;

etl::unique_ptr<IO::Output::LED> LED;

void UserInteractions::Initialize()
{
    inputs = IO::GetInputs();
    LED = IO::GetDefaultLED();
}


[[noreturn]]
void UserInteractions::Entry()
{
    LED->Blink(200);

    uint32_t time = Time::UsSinceBoot();

    while(true) {
        // wait until at least next half of a ms to help with debounce buttons a bit
       if(time + 100 <= Time::UsSinceBoot()) {
           continue;
       }

        for (auto &input : inputs) {
            auto message = input->GetMessage();
            if(message == nullptr)
                continue;
            //send to other core
            Communication::PushOntoMessageQueue(message);
        }

        time = Time::UsSinceBoot();
    }
}

[[maybe_unused]]
void UserInteractions::SOS(IO::Output::LED* led)
{
    const int ms = 200;
    for (int i = 0; i < 9; ++i)
    {
        led->Blink((i > 2 && i < 6) ? ms * 2 : ms, ms);
    }
}




