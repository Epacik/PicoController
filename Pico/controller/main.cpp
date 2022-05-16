#include "etl_profile.h"
#include "main.h"



int main()
{
    bi_decl(bi_program_description("PROGRAM USED TO CONTROL VOLUME IN WINDOWS... OR MORE"));
    stdio_init_all();

    multicore_launch_core1(Communication::Entry);
    UserInteractions::Initialize();
    UserInteractions::Entry();
}