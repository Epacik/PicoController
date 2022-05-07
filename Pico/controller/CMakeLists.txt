add_executable(VolumeControl
    main.cpp main.h
    inputs/Input.h
    inputs/Button.cpp inputs/Button.h
    inputs/Encoder.cpp inputs/Encoder.h
    inputs/EncoderWithButton.cpp inputs/EncoderWithButton.h
    inputs/CreateInputs.h
    Core1.cpp Core1.h
    pins.h
)

pico_enable_stdio_usb(VolumeControl 1)
pico_enable_stdio_uart(VolumeControl 1)

target_link_libraries(VolumeControl 
    pico_stdlib 
    pico_multicore
    hardware_exception
    etl)

pico_add_extra_outputs(VolumeControl)