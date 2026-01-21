param(
    [string]$Configuration = "Release",
    [string]$Filter = "FullyQualifiedName~LeakTests",
    [string]$ResultsDir = "artifacts/dotmemory",
    [string]$DotMemoryLogLevel = "VERBOSE",
    [string]$TestVerbosity = "detailed",
    [string]$TestLogger = "console;verbosity=detailed"
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

if (-not $IsWindows) {
    Write-Error "dotMemory Unit runner is Windows-only; run this script on Windows."
    exit 1
}

$repoRoot = Resolve-Path (Join-Path $PSScriptRoot "..")
$packagesProps = Join-Path $repoRoot "Directory.Packages.props"
if (-not (Test-Path $packagesProps)) {
    throw "Directory.Packages.props not found at $packagesProps"
}

[xml]$packagesXml = Get-Content $packagesProps
$version = ($packagesXml.Project.ItemGroup.PackageVersion |
    Where-Object { $_.Include -eq "JetBrains.dotMemoryUnit" } |
    Select-Object -First 1).Version
if (-not $version) {
    throw "JetBrains.dotMemoryUnit version not found in Directory.Packages.props"
}

$dotMemoryUnit = Join-Path $env:USERPROFILE ".nuget\\packages\\jetbrains.dotmemoryunit\\$version\\lib\\tools\\dotMemoryUnit.exe"
if (-not (Test-Path $dotMemoryUnit)) {
    throw "dotMemoryUnit.exe not found at $dotMemoryUnit. Run 'dotnet restore' first."
}

$dotnet = (Get-Command dotnet).Source
if (-not $dotnet) {
    throw "dotnet not found in PATH."
}

$testProject = Join-Path $repoRoot "src\\Avalonia.Controls.DataGrid.LeakTests\\Avalonia.Controls.DataGrid.LeakTests.csproj"
if (-not (Test-Path $testProject)) {
    throw "Test project not found at $testProject"
}

$resultsDirFull = Join-Path $repoRoot $ResultsDir
New-Item -ItemType Directory -Path $resultsDirFull -Force | Out-Null

$workspaceList = Join-Path $resultsDirFull "dotmemory-workspaces.txt"
$dotMemoryLog = Join-Path $resultsDirFull "dotmemory-unit.log"

$arguments = @(
    $dotnet
    "--work-dir=$repoRoot"
    "--workspace-dir=$resultsDirFull"
    "--output-file=$workspaceList"
    "--propagate-exit-code"
    "--log-level=$DotMemoryLogLevel"
    "--log-file=$dotMemoryLog"
    "--"
    "test"
    $testProject
    "-c"
    $Configuration
    "-v"
    $TestVerbosity
    "--logger"
    $TestLogger
    "--filter"
    $Filter
)

Write-Host "dotMemoryUnit: $dotMemoryUnit"
Write-Host "dotnet: $dotnet"
Write-Host "Test project: $testProject"
Write-Host "dotMemory log: $dotMemoryLog"

& $dotMemoryUnit @arguments
