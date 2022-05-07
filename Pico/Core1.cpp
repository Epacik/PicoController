#include "Core1.h"
#include "hardware/exception.h"

queue_t messageQueue;

union U32ToU8
{
    uint32_t u32;
    uint8_t u8[4];
};

union InputTypeToU8
{
    Inputs::InputType type;
    uint8_t u8[2];
};

void Core1::Main()
{
    multicore_fifo_clear_irq();
    
    //exception_set_exclusive_handler(HARDFAULT_EXCEPTION, Core1::HardErrorHandler);
    queue_init(&messageQueue, sizeof(Inputs::Message*), 100);

    while (true)
    {
        sleep_ms(100);
        if(queue_is_empty(&messageQueue))
            continue;
        
        while(!queue_is_empty(&messageQueue))
        {
            Inputs::Message* msg;
            queue_remove_blocking(&messageQueue, msg);

            U32ToU8 id;
            id.u32 = msg->InputId;

            InputTypeToU8 t;
            t.type = msg->InputType;

            U32ToU8 val;
            val.u32 = msg->Value;

            printf("M;;%s4%s2%s4;\r\n", id.u8, t.u8, val.u8);
            // std::cout << 
            //     'M' << ';' << ';' << 
            //     id.u8[0] << id.u8[1] << id.u8[2] << id.u8[3] << 
            //     t.u8[0] << t.u8[1] << 
            //     val.u8[0] << val.u8[1] << val.u8[2] << val.u8[3] << ';' << std::endl;
            
            delete msg;
        }
    }   
    
}

void Core1::PushOntoMessageQueue(Inputs::Message* message)
{
    queue_add_blocking(&messageQueue, message);
}