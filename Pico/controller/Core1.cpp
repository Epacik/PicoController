#include "Core1.h"
#include "hardware/exception.h"
#include "etl/queue.h"

//queue_t messageQueue;
etl::queue<Inputs::Message*, 100> messageQueue1;

void Core1::Main()
{
    multicore_fifo_clear_irq();

    while (true)
    {
        sleep_ms(100);
        if(messageQueue1.empty() /*queue_is_empty(&messageQueue)*/)
            continue;
        
        while(!messageQueue1.empty() /*queue_is_empty(&messageQueue)*/)
        {
            Inputs::Message* msg;
            messageQueue1.pop_into(msg);

            char outputBuffer[21];

            snprintf(outputBuffer, 21, "M;;%02X%04X%08X;\r\n",
                msg->InputId, // input id
                static_cast<uint16_t>(msg->InputType),
                msg->Value);

            printf(outputBuffer);
            
            delete msg;
        }
    }
}

void Core1::PushOntoMessageQueue(Inputs::Message* message)
{
    messageQueue1.push(message);
}