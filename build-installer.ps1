#Requires -Version 5.1
$ErrorActionPreference = 'Stop'

$root = Split-Path -Parent $MyInvocation.MyCommand.Path
Set-Location $root

$publishDir = Join-Path $root 'publish'
$installerDir = Join-Path $root 'installer'
$msi = Join-Path $installerDir 'DisableMouseSideButtonsSetup.msi'

dotnet publish (Join-Path $root 'src\DisableMouseSideButtons.csproj') `
    -c Release `
    -r win-x64 `
    --self-contained true `
    -p:PublishSingleFile=true `
    -p:IncludeNativeLibrariesForSelfExtract=true `
    -o $publishDir

if (-not (Get-Command wix -ErrorAction SilentlyContinue)) {
    throw 'WiX CLI not found. Install with: dotnet tool install --global wix --version 5.0.2'
}

Set-Location $installerDir
wix build Product.wxs `
    -ext WixToolset.UI.wixext `
    -ext WixToolset.Util.wixext `
    -arch x64 `
    -out $msi

Get-Item $msi | Select-Object FullName, Length, LastWriteTime
