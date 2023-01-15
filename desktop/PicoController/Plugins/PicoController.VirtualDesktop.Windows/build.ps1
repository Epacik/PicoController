param (
    [Parameter(Mandatory = $true, Position = 0)]
    [string]$Out
)

$Out = $Out + "PicoController.VirtualDesktop.Windows"

dotnet publish "$PSScriptRoot\PicoController.VirtualDesktop.Windows.csproj" --use-current-runtime --sc --os win -o "$Out";

robocopy "$PSScriptRoot\thirdparty\" "$Out"
exit 0