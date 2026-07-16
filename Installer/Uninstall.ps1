$ErrorActionPreference = 'Stop'

$installDirectory = Join-Path $env:LOCALAPPDATA 'SystemCheck'
$shortcutPath = Join-Path ([Environment]::GetFolderPath('Startup')) 'SystemCheck.lnk'

if (Test-Path -LiteralPath $shortcutPath) {
    Remove-Item -LiteralPath $shortcutPath -Force
}

Get-Process -Name 'SystemProject' -ErrorAction SilentlyContinue | Stop-Process -Force

if (Test-Path -LiteralPath $installDirectory) {
    Remove-Item -LiteralPath $installDirectory -Recurse -Force
}

Write-Host 'SystemCheck en de automatische opstartinstelling zijn verwijderd.'
