add_executable(PicoController
        main.cpp main.h

        IO/Pins.cpp IO/Pins.h
        IO/Input/Input.h
        IO/Input/Button.cpp IO/Input/Button.h
        IO/Input/Encoder.cpp IO/Input/Encoder.h
        IO/Input/EncoderWithButton.cpp IO/Input/EncoderWithButton.h
        IO/Input/CreateInputs.h

        Communication.cpp Communication.h
        UserInteractions.cpp UserInteractions.h
        pins.h
        IO/Output/LED.cpp IO/Output/LED.h IO/Output/Output.h)

pico_enable_stdio_usb(PicoController 1)
pico_enable_stdio_uart(PicoController 1)

target_link_libraries(PicoController 
    pico_stdlib 
    pico_multicore
    hardware_exception
    etl)

pico_add_extra_outputs(PicoController)