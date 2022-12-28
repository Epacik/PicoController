param (
    [Parameter( Mandatory = $false, Position = 0)]
    [ValidateScript( { $_ -in (Get-ChildItem -Directory -Exclude .vscode).Name }, ErrorMessage = 'Please specify the name of a subdirectory in the current directory.')]
    [string[]]$Plugins
)

if ($Plugins.Count -eq 0) {
    $Plugins = (Get-ChildItem -Directory -Exclude .vscode,bin -Path $PSScriptRoot).Name;
}

foreach ($plugin in $Plugins) {
    dotnet publish $PSScriptRoot/$plugin --use-current-runtime --os win -o $env:userprofile\.picoController\Plugins\$plugin;
    # if ($plugin -eq 'PicoController.VirtualDesktop.Windows') {
    #     robocopy "$PSScriptRoot\..\..\..\Thirdparty\publish\VirtualDesktop\" "$env:userprofile\.picoController\Plugins\$plugin"
    # }
}
