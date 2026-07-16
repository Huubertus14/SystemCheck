$ErrorActionPreference = 'Stop'

$repositoryRoot = $PSScriptRoot
$project = Join-Path $repositoryRoot 'SystemProject\SystemProject.csproj'
$artifacts = Join-Path $repositoryRoot 'artifacts'
$packageRoot = Join-Path $artifacts 'SystemCheck-win-x64'
$appDirectory = Join-Path $packageRoot 'app'
$zipPath = Join-Path $artifacts 'SystemCheck-win-x64.zip'

if (Test-Path -LiteralPath $packageRoot) {
    Remove-Item -LiteralPath $packageRoot -Recurse -Force
}

New-Item -ItemType Directory -Force -Path $appDirectory | Out-Null

dotnet publish $project `
    --configuration Release `
    --runtime win-x64 `
    --self-contained true `
    -p:PublishSingleFile=true `
    -p:DebugType=None `
    -p:DebugSymbols=false `
    --output $appDirectory

if ($LASTEXITCODE -ne 0) {
    throw 'Publiceren van SystemCheck is mislukt.'
}

Copy-Item (Join-Path $repositoryRoot 'Installer\Install.ps1') $packageRoot
Copy-Item (Join-Path $repositoryRoot 'Installer\Install.cmd') $packageRoot
Copy-Item (Join-Path $repositoryRoot 'Installer\Uninstall.ps1') $packageRoot
Copy-Item (Join-Path $repositoryRoot 'Installer\Uninstall.cmd') $packageRoot

if (Test-Path -LiteralPath $zipPath) {
    Remove-Item -LiteralPath $zipPath -Force
}

Compress-Archive -Path (Join-Path $packageRoot '*') -DestinationPath $zipPath
Write-Host "Installatiepakket gemaakt: $zipPath"
