param (
    [Parameter()]
    [string]$Out,
    [Parameter()]
    [switch]$Release
)

if ($null -eq $Out -or "" -eq $Out) {
    $Out = "${PSScriptRoot}/Publish"
}

if ((Test-Path $Out -PathType Container) -eq $false) {
    New-Item -ItemType Directory -Path $Out;
}

$config = if ($Release) { "Release" } else { "Debug" };
 
dotnet publish "${PSScriptRoot}/PicoController.Gui/PicoController.Gui.csproj" `
    -c $config `
    -f net8.0-windows10.0.19041 `
    -o $Out
