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

Pop-Location

Write-Host "Build completed successfully!" -ForegroundColor Green
