
param (
    [Parameter()]
    [string] $InputFile = $null
)

Write-Host "PICO_CONTROLLER_INPUTS is set to $Env:PICO_CONTROLLER_INPUTS"
if (!($InputFile) -and $Env:PICO_CONTROLLER_INPUTS) {
    $InputFile = $Env:PICO_CONTROLLER_INPUTS;
}
elseif (!$InputFile) {
    $InputFile = "default.json";
}

if (!$InputFile.EndsWith(".json")) {
    $InputFile = $InputFile + ".json";
}

$cwd = $PSScriptRoot;
$InputFile = "$cwd/config/$InputFile";
$outputFile = "$cwd/Generated/CurrentIO.cpp";

if (!(Test-Path -Path $InputFile -PathType Leaf)) {
    Write-Error "File does not exist.`nPath: $InputFile";
    exit -1;
}

Write-Host "Creating inputs from $InputFile";

$peripherals = Get-Content $InputFile -Raw 
$peripherals = ConvertFrom-Json $peripherals

$linesInputs = "";
$linesOutputs = "";
$defaultLED = "IO::Output::EmptyLED()";
$Id = 0;

function New-Input([string]$value) {
    return "        inputs.push_back(etl::unique_ptr<IO::Input::Input>(new IO::Input::$value));`n";
}
function New-Output([string]$value) {
    return "        outputs.push_back(etl::unique_ptr<IO::Output::Output>(new IO::Output::$value));`n";
}

foreach ($input in $peripherals.inputs) {
    $pinCount = $input.pins.Length 
    if ($input.type -eq "button") {
        if ($pinCount -ne 1) {
            Write-Error "Invalid pin count, expected 1 got $pinCount";
            continue;
        }
        $pin = [int]$input.pins[0]
        $linesInputs += New-Input("Button($Id, $pin)");
    }
    elseif ($input.type -eq "encoder") {
        if ($pinCount -ne 2) {
            Write-Error "Invalid pin count, expected 2 got $pinCount";
            continue;
        }
        $pinA = [int]$input.pins[0]
        $pinB = [int]$input.pins[1]
        $linesInputs += New-Input("Encoder($Id, $pinA, $pinB)");
    }
    elseif ($input.type -eq "encoderWithButton") {
        if ($pinCount -ne 3) {
            Write-Error "Invalid pin count, expected 3 got $pinCount";
            continue;
        }

        $pinA = [int]$input.pins[0]
        $pinB = [int]$input.pins[1]
        $pinButton = [int]$input.pins[2]
        $linesInputs += New-Input("EncoderWithButton($Id, $pinA, $pinB, $pinButton)");
    }

    $Id += 1;
}

foreach ($output in $peripherals.outputs) {
    $pinCount = $output.pins.Length 
    if ($output.type -eq "defaultLed") {
        if ($pinCount -ne 1) {
            Write-Error "Invalid pin count, expected 1 got $pinCount";
            continue;
        }
        $pin = [int]$output.pins[0]
        $val = "LED($pin)";
        $linesOutputs += New-Output($val);
        $defaultLED = $val;
    }
    elseif ( $output.type -eq "defaultLedPicoW" ) {
        if ($pinCount -ne 0) {
            Write-Error "Invalid pin count, expected 0 got $pinCount";
            continue;
        }
        $val = "PicoWDefaultLED()";
        $linesOutputs += New-Output($val);
        $defaultLED = $val;
    }
}

$hasBattery = "false";
$initBattery = "";
$getBattery = "return 0;"

if ($null -ne $peripherals.batteryInput) {
    $hasBattery = "true";

    $pin = $peripherals.batteryInput.pin;
    $adc = $peripherals.batteryInput.adc;
    $initBattery = 
@"
            adc_init();
            adc_gpio_init($pin);
"@
    $getBattery = 
@"
            adc_select_input($adc);
            auto value = adc_read();
            return (float)(((float)value) / 4095.0 * 3.3);
"@;
}


$linesInputs  = $linesInputs.TrimEnd()
$linesOutputs = $linesOutputs.TrimEnd()

$content = @"
// THIS FILE IS AUTOGENERATED, CHANGES MADE TO IT WILL BE LOST

#include "hardware/adc.h"

#include "../IO/CurrentIO.h"

#include "../IO/Input/Input.h"
#include "../IO/Input/Button.h"
#include "../IO/Input/Encoder.h"
#include "../IO/Input/EncoderWithButton.h"

#include "../IO/Output/LED.h"
#include "../IO/Output/PicoWDefaultLED.h"


namespace IO {

    etl::vector<etl::unique_ptr<IO::Input::Input>, 256> GetInputs()
    {
        etl::vector<etl::unique_ptr<IO::Input::Input>, 256> inputs;

$linesInputs

        return inputs;
    }

    etl::vector<etl::unique_ptr<IO::Output::Output>, 256> GetOutputs() 
    {
        etl::vector<etl::unique_ptr<IO::Output::Output>, 256> outputs;

$linesOutputs
        
        return outputs;
    }

    etl::unique_ptr<IO::Output::LED> GetDefaultLED() {
        return etl::unique_ptr<IO::Output::LED>(new IO::Output::${defaultLED});
    }

    namespace Battery {
        void Initialize() {
$initBattery
        }

        float GetBatteryVoltage() {
$getBattery
        }

        bool HasBattery() {
            return $hasBattery;
        }
    }
}

"@;

$content | Out-File -FilePath $outputFile -Encoding utf8 -Force 

