#include "UserInteractions.h"
#include "IO/Input/CreateInputs.h"
#include "IO/Output/LED.h"
#include "Communication.h"

etl::vector<etl::unique_ptr<IO::Input::Input>, 256> inputs;

IO::Output::LED* LED;

void UserInteractions::Initialize()
{
    inputs = IO::Input::CreateInputs();
    LED = new IO::Output::LED(0, 25);
}


[[noreturn]]
void UserInteractions::Entry()
{
    LED->Blink(200);

    //SOS(LED);
    uint32_t time = UsSinceBoot();

    while(true) {
        // wait until at least next half of a ms to help with debounce buttons a bit
        if(time + 500 >= UsSinceBoot()) {
            continue;
        }

        for (auto &input : inputs) {
            auto message = input->GetMessage();
            if(message == nullptr)
                continue;
            //send to other core
            Communication::PushOntoMessageQueue(message);
        }

        time = UsSinceBoot();
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



uint32_t UserInteractions::UsSinceBoot()
{
    return to_us_since_boot(get_absolute_time());
}



