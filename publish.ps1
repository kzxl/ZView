<#
    publish.ps1 — Optimized Lite Publish script for ZView
    Adheres to AgentOption .NET Publish Release standard & ZeroUniverse rules.
    Lite Mode: Framework-dependent single-file executable for ultra-lightweight distribution.
#>
[CmdletBinding()]
param(
    [string]$Configuration = 'Release',
    [string]$Runtime = 'win-x64',
    [string]$Version = 'v1.0.0'
)

$ErrorActionPreference = 'Stop'
$Root = $PSScriptRoot
$UiProj = Join-Path $Root "src\ZView\ZView.csproj"
$ZUpdateProj = Join-Path $Root "..\ZUpdate\src\ZUpdate\ZUpdate.csproj"
$Dist = Join-Path $Root "publish"
$OutLite = Join-Path $Dist "zview-lite"
$OutRelease = Join-Path $Dist "release"

# Clean up running instances to prevent file-locking during bundling
Get-Process -Name ZView, ZUpdate -ErrorAction SilentlyContinue | Stop-Process -Force -ErrorAction SilentlyContinue
Start-Sleep -Milliseconds 400

Write-Host ">>> Publishing ZView LITE (Framework-Dependent Single File)..." -ForegroundColor Cyan

dotnet publish $UiProj -c $Configuration -r $Runtime --self-contained false `
    -p:PublishSingleFile=true `
    -o $OutLite

# Bundle ZUpdate engine for seamless self-updating
if (Test-Path $ZUpdateProj) {
    Write-Host ">>> Bundling ZUpdate engine..." -ForegroundColor Cyan
    $ZUpdateOut = Join-Path $Root "..\ZUpdate\publish\zupdate-lite"
    dotnet publish $ZUpdateProj -c $Configuration -r $Runtime --self-contained false -p:PublishSingleFile=true -o $ZUpdateOut
    Copy-Item (Join-Path $ZUpdateOut "ZUpdate.exe") (Join-Path $OutLite "ZUpdate.exe") -Force
}

# Package release zip
if (!(Test-Path $OutRelease)) { New-Item -ItemType Directory -Path $OutRelease -Force }
$ZipName = "ZView-$Version-$Runtime-lite.zip"
$ZipPath = Join-Path $OutRelease $ZipName
if (Test-Path $ZipPath) { Remove-Item $ZipPath -Force }

Write-Host ">>> Packaging Release: $ZipPath..." -ForegroundColor Cyan
# Only include binary, languages, and license/readme in distribution
Compress-Archive -Path "$OutLite\ZView.exe", "$OutLite\ZUpdate.exe", "$OutLite\Languages" -DestinationPath $ZipPath -Force

Write-Host "  ✔ ZView Lite generated at: $OutLite\ZView.exe" -ForegroundColor Green
Write-Host "  ✔ Release package generated: $ZipPath" -ForegroundColor Green
Write-Host ">>> ZView publish completed successfully!" -ForegroundColor Green
