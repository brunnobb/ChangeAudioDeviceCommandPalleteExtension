# PowerShell script to generate all MSIX assets using .NET API (no external dependencies required)
Add-Type -AssemblyName System.Drawing

$SourcePath = "C:\Users\brunn\.gemini\antigravity\brain\fe728f04-1caa-4a55-ac53-8167d16be890\switch_volume_logo_1781860096626.png"
$AssetsDir = "c:\Workspace\ChangeAudioDeviceCommandPalleteExtension\ChangeAudioDeviceCommandPalleteExtension\ChangeAudioDeviceCommandPalleteExtension\Assets"

if (-not (Test-Path $SourcePath)) {
    Write-Error "Source image not found at $SourcePath"
    exit 1
}

# Ensure Assets dir exists
if (-not (Test-Path $AssetsDir)) {
    New-Item -ItemType Directory -Path $AssetsDir -Force | Out-Null
}

$sourceImg = [System.Drawing.Image]::FromFile($SourcePath)

function Resize-Image {
    param (
        [string]$Filename,
        [int]$Width,
        [int]$Height,
        [string]$Mode
    )

    Write-Host "Generating $Filename ($Width x $Height)..."
    $destPath = Join-Path $AssetsDir $Filename
    
    $destBitmap = New-Object System.Drawing.Bitmap($Width, $Height)
    $g = [System.Drawing.Graphics]::FromImage($destBitmap)
    
    # Set high quality rendering settings
    $g.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
    $g.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::HighQuality
    $g.PixelOffsetMode = [System.Drawing.Drawing2D.PixelOffsetMode]::HighQuality
    $g.CompositingQuality = [System.Drawing.Drawing2D.CompositingQuality]::HighQuality
    
    if ($Mode -eq "resize") {
        # Draw stretched to target size
        $g.Clear([System.Drawing.Color]::Transparent)
        $g.DrawImage($sourceImg, 0, 0, $Width, $Height)
    }
    elseif ($Mode -eq "pad") {
        # Center the image with padding
        if ($Filename -like "*SplashScreen*") {
            # Dark background for splash screen (#111115)
            $splashColor = [System.Drawing.Color]::FromArgb(255, 17, 17, 21)
            $g.Clear($splashColor)
        } else {
            $g.Clear([System.Drawing.Color]::Transparent)
        }
        
        # Fit inside keeping aspect ratio, target logo height is 60% of canvas height
        $logoH = [int]($Height * 0.6)
        $logoW = $logoH
        
        $offsetX = [int](($Width - $logoW) / 2)
        $offsetY = [int](($Height - $logoH) / 2)
        
        $g.DrawImage($sourceImg, $offsetX, $offsetY, $logoW, $logoH)
    }
    
    $destBitmap.Save($destPath, [System.Drawing.Imaging.ImageFormat]::Png)
    $g.Dispose()
    $destBitmap.Dispose()
}

# Generate all assets
Resize-Image "StoreLogo.png" 50 50 "resize"
Resize-Image "Square150x150Logo.scale-200.png" 300 300 "resize"
Resize-Image "Square44x44Logo.scale-200.png" 88 88 "resize"
Resize-Image "Square44x44Logo.targetsize-24_altform-unplated.png" 24 24 "resize"
Resize-Image "LockScreenLogo.scale-200.png" 48 48 "resize"
Resize-Image "Wide310x150Logo.scale-200.png" 620 300 "pad"
Resize-Image "SplashScreen.scale-200.png" 1240 600 "pad"

$sourceImg.Dispose()
Write-Host "All assets generated successfully!" -ForegroundColor Green
