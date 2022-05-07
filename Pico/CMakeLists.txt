cmake_minimum_required(VERSION 3.13)
include($ENV{PICO_SDK_PATH}/pico_sdk_init.cmake)

project(VolumeControl C CXX ASM)
project(VolumeControlTests C CXX ASM)
set(CMAKE_C_STANDARD 11)
set(CMAKE_CXX_STANDARD 20)

if (PICO_SDK_VERSION_STRING VERSION_LESS "1.3.0")
    message(FATAL_ERROR "Raspberry Pi Pico SDK version 1.3.0 (or later) required. Your version is ${PICO_SDK_VERSION_STRING}")
endif()

pico_sdk_init()

Include(FetchContent)

FetchContent_Declare(
  etl
  GIT_REPOSITORY https://github.com/ETLCPP/etl
  GIT_TAG        e59f995289f1064cb93c6d18596fbbad2d516fb0 #"20.27.3"
)

FetchContent_MakeAvailable(etl)

add_subdirectory(controller)
add_subdirectory(tests)