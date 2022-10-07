#include "Communication.h"
#include "hardware/exception.h"
#include "etl/queue.h"

//queue_t messageQueue;
etl::queue<IO::Input::Message*, 100> messageQueue1;

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
                char outputBuffer[21];

                snprintf(outputBuffer, 21, "Inp%02X%04X%08lX;\r\n",
                    msg->InputId,
                    static_cast<uint16_t>(msg->InputType),
                    msg->Value);

                printf("%s", outputBuffer);
            }
            delete msg;
        }
    }
}

void Communication::PushOntoMessageQueue(IO::Input::Message* message)
{
    messageQueue1.push(message);
}
