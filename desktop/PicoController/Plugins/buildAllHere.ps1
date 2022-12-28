param (
    [Parameter( Mandatory = $false, Position = 0)]
    [ValidateScript( { $_ -in (Get-ChildItem -ErrorAction Stop -Directory -Exclude .vscode,bin).Name }, ErrorMessage = 'Please specify the name of a subdirectory in the current directory.')]
    [string[]]$Plugins = [string[]]@()
)

$pluginDirs = $null;

if ($Plugins.Count -eq 0) {
    try {
        $pluginDirs = [string[]] (Get-ChildItem -ErrorAction Stop -Directory -Exclude .vscode,bin -Path $PSScriptRoot).Name;
    }
    catch {
        Write-Error $_
        exit -1;
    }
}
else {
    $pluginDirs = $Plugins;
}

foreach ($plugin in $pluginDirs) {
    dotnet publish $PSScriptRoot/$plugin --use-current-runtime --sc --os win -o $PSScriptRoot\bin\$plugin;
    if ($plugin -eq 'PicoController.VirtualDesktop.Windows') {
        robocopy "$PSScriptRoot\..\..\..\Thirdparty\publish\VirtualDesktop\" "$PSScriptRoot\bin\PicoController.VirtualDesktop.Windows"
    }
}
