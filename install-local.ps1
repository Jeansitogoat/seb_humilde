# Instala Safe Exam Browser (build local) en tu PC, como aplicacion de escritorio.
# No requiere admin: usa %LOCALAPPDATA%\Programs\SafeExamBrowser
# Uso: .\install-local.ps1
#      .\install-local.ps1 -SkipBuild
#      .\install-local.ps1 -System   (requiere admin, instala en Program Files)

param(
    [switch]$SkipBuild,
    [switch]$System,
    [string]$InstallDir = ""
)

$ErrorActionPreference = "Stop"
$Root = Split-Path -Parent $MyInvocation.MyCommand.Path
$SourceDir = Join-Path $Root "SafeExamBrowser.Runtime\bin\x64\Release"
$ExeName = "SafeExamBrowser.exe"

if (-not $SkipBuild) {
    Write-Host "Compilando Release x64..."
    & (Join-Path $Root "build-release-x64.ps1")
}

if (-not (Test-Path (Join-Path $SourceDir $ExeName))) {
    throw "No se encontro $ExeName en $SourceDir. Ejecuta .\build-release-x64.ps1 primero."
}

if ([string]::IsNullOrWhiteSpace($InstallDir)) {
    if ($System) {
        $InstallDir = Join-Path ${env:ProgramFiles} "SafeExamBrowser"
    } else {
        $InstallDir = Join-Path $env:LOCALAPPDATA "Programs\SafeExamBrowser"
    }
}

Write-Host "Instalando en: $InstallDir"

if (Test-Path $InstallDir) {
    Write-Host "Actualizando instalacion existente..."
} else {
    New-Item -ItemType Directory -Path $InstallDir -Force | Out-Null
}

robocopy $SourceDir $InstallDir /MIR /NP /NFL /NDL /NJH /NJS | Out-Null
if ($LASTEXITCODE -ge 8) {
    throw "Error copiando archivos (robocopy exit code $LASTEXITCODE)"
}

$SebExe = Join-Path $InstallDir $ExeName
$IconPath = Join-Path $InstallDir "SafeExamBrowser.exe"

# Acceso directo en Escritorio
$Desktop = [Environment]::GetFolderPath("Desktop")
$DesktopLnk = Join-Path $Desktop "Safe Exam Browser.lnk"
$Wsh = New-Object -ComObject WScript.Shell
$Shortcut = $Wsh.CreateShortcut($DesktopLnk)
$Shortcut.TargetPath = $SebExe
$Shortcut.WorkingDirectory = $InstallDir
$Shortcut.IconLocation = "$IconPath,0"
$Shortcut.Description = "Safe Exam Browser (build local - auditoria)"
$Shortcut.Save()
Write-Host "Acceso directo: $DesktopLnk"

# Menu Inicio
$StartMenu = Join-Path $env:APPDATA "Microsoft\Windows\Start Menu\Programs\SafeExamBrowser"
New-Item -ItemType Directory -Path $StartMenu -Force | Out-Null
$StartLnk = Join-Path $StartMenu "Safe Exam Browser.lnk"
$Shortcut2 = $Wsh.CreateShortcut($StartLnk)
$Shortcut2.TargetPath = $SebExe
$Shortcut2.WorkingDirectory = $InstallDir
$Shortcut2.IconLocation = "$IconPath,0"
$Shortcut2.Description = "Safe Exam Browser (build local - auditoria)"
$Shortcut2.Save()
Write-Host "Menu Inicio: $StartLnk"

# Asociar archivos .seb (solo usuario actual, sin admin)
$SebConfig = Join-Path $Root "examen_prueba.seb"
if (Test-Path $SebConfig) {
    Copy-Item $SebConfig (Join-Path $InstallDir "examen_prueba.seb") -Force
}

function Register-SebHandler {
    param(
        [string]$Key,
        [string]$Description,
        [switch]$IsUrlProtocol
    )

    reg add "HKCU\Software\Classes\$Key" /ve /d $Description /f | Out-Null

    if ($IsUrlProtocol) {
        reg add "HKCU\Software\Classes\$Key" /v "URL Protocol" /d "" /f | Out-Null
    }

    reg add "HKCU\Software\Classes\$Key\DefaultIcon" /ve /d "$IconPath,0" /f | Out-Null
    reg add "HKCU\Software\Classes\$Key\shell\open\command" /ve /d "`"$SebExe`" `"%1`"" /f | Out-Null
}

# .seb descargados (doble clic / abrir con)
reg add "HKCU\Software\Classes\.seb" /ve /d "SafeExamBrowserConfig" /f | Out-Null
Register-SebHandler -Key "SafeExamBrowserConfig" -Description "SEB Configuration File"

# Mismo ProgId que usa el instalador oficial (por si Windows lo prefiere)
Register-SebHandler -Key "ConfigurationFileExtension" -Description "SEB Configuration File"

# Enlaces del portal de examenes y Moodle (seb:// / sebs://)
Register-SebHandler -Key "seb" -Description "URL:Safe Exam Browser Protocol" -IsUrlProtocol
Register-SebHandler -Key "sebs" -Description "URL:Safe Exam Browser Secure Protocol" -IsUrlProtocol

Write-Host "Asociaciones configuradas en HKCU (prioridad sobre SEB oficial):"
Write-Host "  - archivos .seb"
Write-Host "  - enlaces seb:// y sebs://"

Write-Host ""
Write-Host "=== Instalacion completada ==="
Write-Host "Ejecutable: $SebExe"
Write-Host ""
Write-Host "Abrir SEB:"
Write-Host "  - Doble clic en 'Safe Exam Browser' del Escritorio"
Write-Host "  - Doble clic en un archivo .seb"
Write-Host "  - Enlaces seb:// / sebs:// del portal de examenes (usa este build, no el oficial)"
Write-Host ""
Write-Host "IMPORTANTE: Cierra el SEB oficial si estaba abierto antes de probar un enlace."
Write-Host ""
Write-Host "Probar con config de ejemplo:"
Write-Host "  & `"$SebExe`" `"$(Join-Path $InstallDir 'examen_prueba.seb')`""
