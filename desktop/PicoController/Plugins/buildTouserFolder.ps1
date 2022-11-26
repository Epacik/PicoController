param (
    [Parameter( Mandatory = $false, Position = 0)]
    [ValidateScript( { $_ -in (Get-ChildItem -Directory -Exclude .vscode).Name }, ErrorMessage = 'Please specify the name of a subdirectory in the current directory.')]
    [string[]]$Plugins
)

if ($Plugins.Count -eq 0) {
    $Plugins = (Get-ChildItem -Directory -Exclude .vscode).Name;
}

foreach ($plugin in $Plugins) {
    dotnet publish ./$plugin --self-contained true -o $env:userprofile\.picoController\Plugins\$plugin;
}
