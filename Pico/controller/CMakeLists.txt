include(CMakePrintHelpers)

add_executable(PicoController
        main.cpp main.h

        IO/Pins.cpp IO/Pins.h
        IO/Input/Input.h
        IO/Input/Button.cpp IO/Input/Button.h
        IO/Input/Encoder.cpp IO/Input/Encoder.h
        IO/Input/EncoderWithButton.cpp IO/Input/EncoderWithButton.h
        Generated/CurrentIO.cpp IO/CurrentIO.h

        Communication/Communication.cpp Communication/Communication.h
        UserInteractions.cpp UserInteractions.h
        IO/Output/Output.h
        IO/Output/LED.cpp IO/Output/LED.h
        IO/Output/PicoWDefaultLED.cpp IO/Output/PicoWDefaultLED.h Time.h Time.cpp)

pico_enable_stdio_usb(PicoController 1)
pico_enable_stdio_uart(PicoController 1)

target_link_libraries(PicoController
    pico_stdlib 
    pico_multicore
    hardware_adc
    hardware_exception
    etl
    mpark_patterns
)

if (DEFINED ENV{BUILD_FOR_PICO_W})

    if (PICO_CYW43_SUPPORTED)
        if (NOT TARGET pico_cyw43_arch)
            message("Skipping Pico W as support is not available")
        else()
            message("Pico W support is available")
            message("Building for PICO W")
            add_compile_definitions(BOARD_PICO_W)

            pico_add_extra_outputs(PicoController)
            target_link_libraries(PicoController
                    pico_cyw43_arch_none     # we need Wifi to access the GPIO, but we don't need anything else
            )
        endif()
    endif()

endif ()

pico_add_extra_outputs(PicoController)

add_custom_command(TARGET PicoController
        PRE_BUILD
        COMMAND pwsh ${CMAKE_CURRENT_SOURCE_DIR}/generate_inputs.ps1
)