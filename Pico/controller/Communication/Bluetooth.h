#pragma once

#include "etl/string.h"


class Bluetooth {
public:
    static void Initialize(bool hasBattery);
    static bool IsInitialized();

    static void PushMessage(etl::string<20> buffer);
};
