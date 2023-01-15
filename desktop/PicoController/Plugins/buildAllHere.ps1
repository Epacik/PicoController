param (
    [Parameter( Mandatory = $false, Position = 0)]
    [ValidateScript( { $_ -in (Get-ChildItem -ErrorAction Stop -Directory -Exclude .vscode,bin).Name }, ErrorMessage = 'Please specify the name of a subdirectory in the current directory.')]
    [string[]]$Plugins = [string[]]@()
)

$base = $PSScriptRoot
if ($null -eq $Plugins) {
    &"$PSScriptRoot/build.ps1" -Out $base
}
else {
    &"$PSScriptRoot/build.ps1" -Out $base -Plugins $Plugins
}
