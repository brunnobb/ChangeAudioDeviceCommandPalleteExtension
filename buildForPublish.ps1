# PowerShell script to build the packaged extension for both x64 and ARM64 platforms
# This generates Appx/MSIX packages for publish/sideloading.

$CsprojFolder = Join-Path $PSScriptRoot "ChangeAudioDeviceCommandPalleteExtension"

if (-not (Test-Path $CsprojFolder)) {
    Write-Error "Csproj folder not found at $CsprojFolder"
    exit 1
}

Push-Location $CsprojFolder

Write-Host "Building for x64 (Release)..." -ForegroundColor Cyan
dotnet build --configuration Release -p:GenerateAppxPackageOnBuild=true -p:Platform=x64 -p:AppxPackageDir="AppPackages\x64\"

Write-Host "Building for ARM64 (Release)..." -ForegroundColor Cyan
dotnet build --configuration Release -p:GenerateAppxPackageOnBuild=true -p:Platform=ARM64 -p:AppxPackageDir="AppPackages\ARM64\"

Write-Host "Locating built MSIX files..." -ForegroundColor Cyan
$csprojPath = Get-ChildItem -Filter *.csproj | Select-Object -First 1
if ($null -eq $csprojPath) {
    Write-Error "No .csproj file found in $CsprojFolder"
    exit 1
}
$ExtensionName = [System.IO.Path]::GetFileNameWithoutExtension($csprojPath.Name)

[xml]$csprojXml = Get-Content $csprojPath.FullName
$VersionNumber = ($csprojXml.Project.PropertyGroup | Where-Object { $_.AppxPackageVersion } | Select-Object -First 1).AppxPackageVersion
if (-not $VersionNumber) {
    $VersionNumber = "1.0.0.0"
}

# Locate MSIX files
$x64Msix = Get-ChildItem -Path "AppPackages" -Recurse -Filter "$($ExtensionName)_$($VersionNumber)_x64.msix" -ErrorAction SilentlyContinue | Select-Object -First 1
if ($null -eq $x64Msix) {
    $x64Msix = Get-ChildItem -Path "bin" -Recurse -Filter "$($ExtensionName)_$($VersionNumber)_x64.msix" -ErrorAction SilentlyContinue | Select-Object -First 1
}

$arm64Msix = Get-ChildItem -Path "AppPackages" -Recurse -Filter "$($ExtensionName)_$($VersionNumber)_arm64.msix" -ErrorAction SilentlyContinue | Select-Object -First 1
if ($null -eq $arm64Msix) {
    $arm64Msix = Get-ChildItem -Path "bin" -Recurse -Filter "$($ExtensionName)_$($VersionNumber)_arm64.msix" -ErrorAction SilentlyContinue | Select-Object -First 1
}

if ($null -eq $x64Msix -or $null -eq $arm64Msix) {
    Write-Error "Could not find one or both MSIX files."
    Write-Host "x64 MSIX found: $(if($x64Msix){$x64Msix.FullName}else{'No'})" -ForegroundColor Yellow
    Write-Host "ARM64 MSIX found: $(if($arm64Msix){$arm64Msix.FullName}else{'No'})" -ForegroundColor Yellow
    exit 1
}

$x64Relative = (Resolve-Path -Path $x64Msix.FullName -Relative) -replace '^\.\\', ''
$arm64Relative = (Resolve-Path -Path $arm64Msix.FullName -Relative) -replace '^\.\\', ''

Write-Host "Creating bundle_mapping.txt..." -ForegroundColor Cyan
$mappingContent = @"
[Files]
"$x64Relative" "$($ExtensionName)_$($VersionNumber)_x64.msix"
"$arm64Relative" "$($ExtensionName)_$($VersionNumber)_arm64.msix"
"@

$mappingPath = Join-Path (Get-Location) "bundle_mapping.txt"
Set-Content -Path $mappingPath -Value $mappingContent
Write-Host "Created bundle_mapping.txt at $mappingPath" -ForegroundColor Green

# Find makeappx.exe
$makeAppx = Get-Command makeappx -ErrorAction SilentlyContinue | Select-Object -ExpandProperty Source
if (-not $makeAppx) {
    $makeAppx = Get-ChildItem "C:\Program Files (x86)\Windows Kits\10\bin\*\x64\makeappx.exe" -ErrorAction SilentlyContinue |
                Sort-Object FullName -Descending |
                Select-Object -ExpandProperty FullName -First 1
}

if (-not $makeAppx) {
    Write-Error "makeappx.exe not found on the system. Please install the Windows SDK."
    exit 1
}

$bundleName = "$($ExtensionName)_$($VersionNumber)_Bundle.msixbundle"
Write-Host "Creating bundle $bundleName..." -ForegroundColor Cyan
& $makeAppx bundle /v /o /f "bundle_mapping.txt" /p $bundleName

if ($LASTEXITCODE -eq 0) {
    Write-Host "Bundle created successfully at $(Join-Path (Get-Location) $bundleName)" -ForegroundColor Green
} else {
    Write-Error "Failed to create bundle."
    exit 1
}

Pop-Location

Write-Host "Build completed successfully!" -ForegroundColor Green

