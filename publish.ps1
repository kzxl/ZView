<#
    publish.ps1 — Optimized Lite Publish script for ZView
    Adheres to AgentOption .NET Publish Release standard & ZeroUniverse rules.
    Lite Mode: Framework-dependent single-file executable for ultra-lightweight distribution.
#>
[CmdletBinding()]
param(
    [string]$Configuration = 'Release',
    [string]$Runtime = 'win-x64'
)

$ErrorActionPreference = 'Stop'
$Root = $PSScriptRoot
$UiProj = Join-Path $Root "src\ZView\ZView.csproj"
$Dist = Join-Path $Root "publish"
$OutLite = Join-Path $Dist "zview-lite"

Write-Host ">>> Publishing ZView LITE (Framework-Dependent Single File)..." -ForegroundColor Cyan

dotnet publish $UiProj -c $Configuration -r $Runtime --self-contained false `
    -p:PublishSingleFile=true `
    -o $OutLite

Write-Host "  ✔ ZView Lite generated at: $OutLite\ZView.exe" -ForegroundColor Green
Write-Host ">>> ZView publish completed successfully!" -ForegroundColor Green
