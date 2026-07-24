$ErrorActionPreference = "Stop"

$fw = "$env:USERPROFILE\.nuget\packages\microsoft.netframework.referenceassemblies.net48\1.0.3\build\.NETFramework\v4.8\"
$msbuild = "${env:ProgramFiles(x86)}\Microsoft Visual Studio\2022\BuildTools\MSBuild\Current\Bin\MSBuild.exe"

if (-not (Test-Path $msbuild)) {
    throw "MSBuild not found at $msbuild"
}

if (-not (Test-Path $fw)) {
    throw ".NET 4.8 reference assemblies not found. Run: dotnet restore on the solution first."
}

$buildArgs = @(
    "/t:Build",
    "/p:Configuration=Release",
    "/p:Platform=x64",
    "/p:FrameworkPathOverride=$fw",
    "/p:TargetFrameworkVersion=v4.8",
    "/v:minimal",
    "/m"
)

$projects = @(
    "SafeExamBrowser.Browser\SafeExamBrowser.Browser.csproj",
    "SafeExamBrowser.Client\SafeExamBrowser.Client.csproj",
    "SafeExamBrowser.Runtime\SafeExamBrowser.Runtime.csproj"
)

foreach ($project in $projects) {
    Write-Host "Building $project..."
    & $msbuild $project @buildArgs
    if ($LASTEXITCODE -ne 0) {
        throw "Build failed for $project (exit code $LASTEXITCODE)"
    }
}

Write-Host ""
Write-Host "Build complete. Key binaries:"
$artifacts = @(
    "SafeExamBrowser.Runtime\bin\x64\Release\SafeExamBrowser.exe",
    "SafeExamBrowser.Runtime\bin\x64\Release\SafeExamBrowser.Client.exe",
    "SafeExamBrowser.Runtime\bin\x64\Release\SafeExamBrowser.Browser.dll",
    "SafeExamBrowser.Runtime\bin\x64\Release\SafeExamBrowser.Monitoring.dll",
    "SafeExamBrowser.Runtime\bin\x64\Release\SafeExamBrowser.Configuration.dll"
)

foreach ($path in $artifacts) {
    if (Test-Path $path) {
        Get-Item $path | Select-Object FullName, LastWriteTime, Length
    } else {
        Write-Warning "Missing: $path"
    }
}
