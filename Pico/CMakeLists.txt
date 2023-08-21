cmake_minimum_required(VERSION 3.13)
set(CMAKE_EXPORT_COMPILE_COMMANDS 1)
include($ENV{PICO_SDK_PATH}/pico_sdk_init.cmake)

project(PicoController C CXX ASM)
project(PicoControllerTests C CXX ASM)
set(CMAKE_C_STANDARD 11)
set(CMAKE_CXX_STANDARD 23)

if (PICO_SDK_VERSION_STRING VERSION_LESS "1.5.1")
    message(FATAL_ERROR "Raspberry Pi Pico SDK version 1.5.1 (or later) required. Your version is ${PICO_SDK_VERSION_STRING}")
endif()

pico_sdk_init()

Include(FetchContent)

FetchContent_Declare(
  etl
  GIT_REPOSITORY https://github.com/ETLCPP/etl
  GIT_TAG        e59f995289f1064cb93c6d18596fbbad2d516fb0 #"20.27.3"
)


FetchContent_Declare(
    mpark_patterns
    GIT_REPOSITORY https://github.com/mpark/patterns
    GIT_TAG b3270e0
)

FetchContent_MakeAvailable(
        etl
        mpark_patterns
)

add_subdirectory(controller)
add_subdirectory(tests)