param (
    [Parameter(Mandatory = $true, Position = 0)]
    [string]$Out,

    [Parameter( Mandatory = $false, Position = 1)]
    [ValidateScript( { $_ -in (Get-ChildItem -Path $PSScriptRoot -ErrorAction Stop -Directory -Exclude .vscode,bin).Name }, ErrorMessage = 'Please specify the name of a subdirectory in the current directory.')]
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
    if (Test-Path -Path "$PSScriptRoot/$plugin/build.ps1")
    {
        &"$PSScriptRoot/$plugin/build.ps1" -Out "$Out$plugin"
    }
    else 
    {
        dotnet publish $PSScriptRoot/$plugin --use-current-runtime --sc --os win -o "$Out$plugin";
    }
}
