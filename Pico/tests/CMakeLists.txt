set(PICO_SDK_TESTS_ENABLED 1)


add_executable(VolumeControlTests
    main.cpp main.h
)

pico_enable_stdio_usb(VolumeControlTests 1)
pico_enable_stdio_uart(VolumeControlTests 1)

target_link_libraries(VolumeControlTests 
    pico_stdlib 
    pico_multicore
    hardware_exception
    etl
    )

pico_add_extra_outputs(VolumeControlTests)