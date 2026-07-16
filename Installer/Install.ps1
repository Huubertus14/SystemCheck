$ErrorActionPreference = 'Stop'

$sourceApp = Join-Path $PSScriptRoot 'app'
$installDirectory = Join-Path $env:LOCALAPPDATA 'SystemCheck'
$executable = Join-Path $installDirectory 'SystemProject.exe'
$startupDirectory = [Environment]::GetFolderPath('Startup')
$shortcutPath = Join-Path $startupDirectory 'SystemCheck.lnk'

if (-not (Test-Path -LiteralPath (Join-Path $sourceApp 'SystemProject.exe'))) {
    throw 'SystemProject.exe ontbreekt. Pak eerst het volledige installatiepakket uit.'
}

New-Item -ItemType Directory -Force -Path $installDirectory | Out-Null
Copy-Item -Path (Join-Path $sourceApp '*') -Destination $installDirectory -Recurse -Force

$shell = New-Object -ComObject WScript.Shell
$shortcut = $shell.CreateShortcut($shortcutPath)
$shortcut.TargetPath = $executable
$shortcut.WorkingDirectory = $installDirectory
$shortcut.Description = 'PC-hardwarecontrole bij aanmelden bij Windows'
$shortcut.Save()

Write-Host 'SystemCheck is geïnstalleerd.'
Write-Host "Programma: $executable"
Write-Host "Automatisch opstarten: $shortcutPath"

Start-Process -FilePath $executable -WorkingDirectory $installDirectory
