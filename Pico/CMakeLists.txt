cmake_minimum_required(VERSION 3.13)

include(pico_sdk_import.cmake)

project(VolumeControl)

pico_sdk_init()

add_executable(VolumeControl    
    main.c main.h
    pins.h
    encoder.c encoder.h
)

pico_enable_stdio_usb(VolumeControl 1)
pico_enable_stdio_uart(VolumeControl 1)

pico_add_extra_outputs(VolumeControl)

target_link_libraries(VolumeControl pico_stdlib)