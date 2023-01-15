param (
    [Parameter(Mandatory = $false, Position = 0)]
    [string]$Config = "Debug"
)

if ($Config -eq "Release"){
    &"$PSScriptRoot\buildTouserFolder.ps1"
}
&"$PSScriptRoot\buildAllHere.ps1"