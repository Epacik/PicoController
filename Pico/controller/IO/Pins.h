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

    class IPin {

        protected:
        const uint8_t pin;
        const PinDirection direction;
        const PinPull pull;
        
        virtual void Init() = 0;
        IPin(uint8_t pin, PinDirection direction, PinPull pull) : pin(pin), direction(direction), pull(pull) {}
    };

    class IInputPin : public IPin
    {
        public:
        virtual bool Read() = 0;

        protected:
        IInputPin(uint8_t pin, PinPull pull) : IPin(pin, PinDirection::Input, pull) {}
    };

    class IOutputPin : public IPin 
    {
        public:
        virtual void Set(bool value) = 0;

        protected:
        IOutputPin(uint8_t pin, PinPull pull) : IPin(pin, PinDirection::Output, pull) {}
    };

    class InputPin : public IInputPin 
    {
        public: 
        explicit InputPin(uint8_t pin, PinPull pull = PinPull::None) : IInputPin(pin, pull) {
            Init();
        }
        bool Read() override;

        protected:
        void Init() override;
        
    };

    class OutputPin : public IOutputPin 
    {
        public:
        explicit OutputPin(uint8_t pin, PinPull pull = PinPull::None) : IOutputPin(pin, pull) {
            Init();
        }
        void Set(bool value) override;

        protected:
        void Init() override;
    };
}