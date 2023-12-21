param (
    [Parameter(Mandatory = $false, Position = 0)]
    [string]$Config = "Debug"
)

if ($Config -eq "Release") {
    &"$PSScriptRoot\buildTouserFolder.ps1"
}
else {
    &"$PSScriptRoot\buildAllHere.ps1"
}

exit 0;