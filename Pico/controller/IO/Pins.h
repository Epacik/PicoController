#pragma once
#include "hardware/gpio.h"
#include <cstdio>
#include "etl/debounce.h"


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
        
        virtual void Init(bool softDebounce);
        Pin(uint8_t pin, PinDirection direction, PinPull pull, bool softDebounce) : pin(pin), direction(direction), pull(pull) {
            Init(softDebounce);
        }

    public:
        virtual void AddState(bool b);
    };

    class InputPin : public Pin
    {
        public: 
        explicit InputPin(
                uint8_t pin,
                PinPull pull = PinPull::None,
                bool softDebounce = false)
                : Pin(
                        pin,
                        PinDirection::Input,
                        pull,
                        softDebounce) {}
        bool Read();
        void AddState(bool b) override;
        void Init(bool softDebounce) override;
    private:
        etl::debounce<> _debounce = etl::debounce<>();
    };

    class OutputPin : public Pin
    {
        public:
        explicit OutputPin(uint8_t pin, PinPull pull = PinPull::None) : Pin(pin, PinDirection::Output, pull, false) { }
        virtual void Set(bool value);
    };
}