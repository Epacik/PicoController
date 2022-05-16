set(PICO_SDK_TESTS_ENABLED 1)


add_executable(PicoControllerTests
    main.cpp main.h
    thirdparty/minunit/minunit.h
)

pico_enable_stdio_usb(PicoControllerTests 1)
pico_enable_stdio_uart(PicoControllerTests 1)

target_link_libraries(PicoControllerTests 
    pico_stdlib 
    pico_multicore
    hardware_exception
    etl
    )

pico_add_extra_outputs(PicoControllerTests)