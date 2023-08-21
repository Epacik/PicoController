#pragma once
#include "pico/stdlib.h"
#include "pico/multicore.h"
#include "hardware/irq.h"
#include "pico/util/queue.h"

#include "../IO/Input/Input.h"


class Communication
{
public:
    [[noreturn]]
    static void Entry();
    static void PushOntoMessageQueue(IO::Input::Message* message);

    static void Initialize(bool hasBattery);
    //static void HardErrorHandler();
};