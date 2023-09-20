param (
    [Parameter(Mandatory = $true, Position = 0)]
    [string]$Out
)

dotnet publish "$PSScriptRoot\PicoController.VirtualDesktop.Windows.csproj" --use-current-runtime --sc --os win -o "$Out";

$mainDeps = Get-ChildItem -Path "$PSScriptRoot/thirdparty" -File

foreach ($dep in $mainDeps) {
    $source = $dep.FullName;
    $name = $dep.Name;
    $dest = "$Out\$name"
    copy-item -Path "$source" -Destination "$dest"
}

$build = [System.Environment]::OSVersion.Version.Build;
$osSpecificDepsFolder = switch ($build) {
    {$PSItem -lt 20000} { "win10" }
    {$PSItem -lt 22621} { "win11" }
    {$PSItem -gt 22621} { "win1123h2" }
};

#if ($build -ge 20000) { "win11" } else { "win10" }

$osSpecificDeps = Get-ChildItem -Path "$PSScriptRoot/thirdparty/$osSpecificDepsFolder" -File
foreach ($dep in $osSpecificDeps) {
    $source = $dep.FullName;
    $name = $dep.Name;
    $dest = "$Out\$name"
    copy-item -Path "$source" -Destination "$dest"
}

robocopy "$PSScriptRoot\thirdparty\" "$Out"
exit 0