param (
    [Parameter( Mandatory = $false, Position = 0)]
    [ValidateScript( { $_ -in (Get-ChildItem -Directory -Exclude .vscode).Name }, ErrorMessage = 'Please specify the name of a subdirectory in the current directory.')]
    [string[]]$Plugins
)

$base = "$env:userprofile\.picoController\Plugins\"
if ($null -eq $Plugins) {
    &"$PSScriptRoot/build.ps1" -Out $base
}
else {
    &"$PSScriptRoot/build.ps1" -Out $base -Plugins $Plugins
}
