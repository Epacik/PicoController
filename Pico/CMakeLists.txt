cmake_minimum_required(VERSION 3.13)

include($ENV{PICO_SDK_PATH}/pico_sdk_init.cmake)

project(VolumeControl C CXX ASM)
set(CMAKE_C_STANDARD 11)
set(CMAKE_CXX_STANDARD 20)

if (PICO_SDK_VERSION_STRING VERSION_LESS "1.3.0")
    message(FATAL_ERROR "Raspberry Pi Pico SDK version 1.3.0 (or later) required. Your version is ${PICO_SDK_VERSION_STRING}")
endif()

pico_sdk_init()

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
    hardware_exception)

pico_add_extra_outputs(VolumeControl)