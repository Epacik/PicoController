#pragma once
#include "hardware/gpio.h"
#include <cstdio>


namespace IO {
    enum class PinDirection 
    {
        Input = false,
        Output = true,
    };

    enum class PinPull
    {
        None = 0,
        Up = 1,
        Down = 2,    
    };

    inline PinPull operator|(PinPull a, PinPull b)
    {
        return static_cast<PinPull>(static_cast<int>(a) | static_cast<int>(b));
    }
    inline PinPull operator&(PinPull a, PinPull b)
    {
        return static_cast<PinPull>(static_cast<int>(a) & static_cast<int>(b));
    }

    class Pin {
        protected:
        const uint8_t pin;
        const PinDirection direction;
        const PinPull pull;
        
        void Init();
        Pin(uint8_t pin, PinDirection direction, PinPull pull) : pin(pin), direction(direction), pull(pull) {
            Init();
        }
    };



    class InputPin : public Pin
    {
        public: 
        explicit InputPin(uint8_t pin, PinPull pull = PinPull::None) : Pin(pin, PinDirection::Input, pull) { }
        bool Read();
    };

    class OutputPin : public Pin
    {
        public:
        explicit OutputPin(uint8_t pin, PinPull pull = PinPull::None) : Pin(pin, PinDirection::Output, pull) { }
        virtual void Set(bool value);
    };
}