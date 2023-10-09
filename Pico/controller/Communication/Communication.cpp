#include "pico/cyw43_arch.h"
#include "Communication.h"
#include "hardware/exception.h"
#include "etl/queue.h"
#include "../IO/CurrentIO.h"
#include "../Time.h"
#include "../UserInteractions.h"
#include "Bluetooth.h"
#include "etl/string_stream.h"


//queue_t messageQueue;
etl::queue<IO::Input::Message*, 100> messageQueue1;
bool hasBattery = false;

uint32_t count = 1;



bool CanTransmitOverUART()
{
#ifndef BOARD_PICO_W
    return true;
#else
    return cyw43_arch_gpio_get(CYW43_WL_GPIO_VBUS_PIN);
#endif
}

[[noreturn]]
void Communication::Entry()
{
    multicore_fifo_clear_irq();

    while (true)
    {
        //sleep_ms(100);
        
        if(!messageQueue1.empty())
        {
            IO::Input::Message* msg;
            messageQueue1.pop_into(msg);
            if(msg == nullptr)
                continue;

            if(msg->InputType <= IO::Input::InputType::MAX)
            {
                etl::string<20> value;
                etl::string_stream str(value);

                str << "Inp";
                str << etl::format_spec().hex().width(2).fill('0') << msg->InputId;
                str << etl::format_spec().hex().width(4).fill('0') << static_cast<uint16_t>(msg->InputType);
                str << etl::format_spec().hex().width(8).fill('0') << msg->Value;
                str << etl::format_spec() << ";\r\n";

                printf("%s", value.c_str());

                if (Bluetooth::IsInitialized())
                {
                   Bluetooth::PushMessage(value);
                }
            }
            delete msg;
        }

//        auto second = SecSinceBoot();
//        auto usec = UsSinceBoot();
//
//        if (second >= 5 && second < 20 && hasBattery && time + 500 <= usec) {
//            auto voltage = IO::GetBatteryVoltage();
//
//            char outputBuffer[17];
//
//            snprintf(outputBuffer, 17, "%02ld,%06ld:%04hX\r\n",
//                     second,
//                     usec % 1000000,
//                     voltage);
//
//            printf("%s", outputBuffer);
//            time = UsSinceBoot();
//        }
    }
}

void Communication::PushOntoMessageQueue(IO::Input::Message* message)
{
    messageQueue1.push(message);
}

void Communication::Initialize(bool battery) {
    hasBattery = battery;

#ifdef BOARD_PICO_W
    Bluetooth::Initialize(battery);
#endif
}




