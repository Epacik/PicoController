param (
    [Parameter(Mandatory = $true, Position = 0)]
    [string]$Out
)

dotnet publish "$PSScriptRoot\PicoController.VirtualDesktop.Windows.csproj" --use-current-runtime --sc --os win -o "$Out";

robocopy "$PSScriptRoot\thirdparty\" "$Out"
exit 0