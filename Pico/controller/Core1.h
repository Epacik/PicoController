#pragma once
#include "pico/stdlib.h"
#include "pico/multicore.h"
#include "hardware/irq.h"
#include "pico/util/queue.h"


#include "inputs/Input.h"


struct Core1
{
    static void Main();
    static void PushOntoMessageQueue(Inputs::Message* message);

    private: 
    //static void HardErrorHandler();
};