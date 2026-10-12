param([string]$buildConfiguration = "Release")

$ErrorActionPreference = "Stop"

$repoRoot = Resolve-Path "$PSScriptRoot\.."
$infraProject = "$repoRoot\Infrastructure.DataAccess\Infrastructure.DataAccess.csproj"
$startupProject = "$repoRoot\Presentation.Web\Presentation.Web.csproj"
$bundleDir = "$repoRoot\Output\MigrationsBundle"
$postgresBundleExe = "$bundleDir\efbundle.postgresql.exe"

New-Item -ItemType Directory -Path $bundleDir -Force | Out-Null

Write-Host "Ensuring dotnet-ef tool is available..."
dotnet tool update --global dotnet-ef
if ($LASTEXITCODE -ne 0) { Throw "Failed to install/update dotnet-ef tool" }

# dotnet tool install/update writes to %USERPROFILE%\.dotnet\tools which may not be on
# PATH yet in this agent session - add it so the 'dotnet ef' command can be resolved.
$env:PATH = "$env:USERPROFILE\.dotnet\tools;$env:PATH"

function New-MigrationsBundle(
    [string]$provider,
    [string]$connectionString,
    [string]$outputPath
) {
    Write-Host "Creating EF Core migrations bundle for $provider at $outputPath"

    $env:Database__Provider = $provider
    $env:ConnectionStrings__KitosContext = $connectionString

    dotnet ef migrations bundle `
        --project "$infraProject" `
        --startup-project "$startupProject" `
        --output "$outputPath" `
        --configuration $buildConfiguration `
        --force

    if ($LASTEXITCODE -ne 0) {
        Throw "Failed to create EF Core migrations bundle for $provider"
    }
}

try {
    # Supply a dummy connection string so the design-time factory can instantiate KitosContext
    # during bundle creation. The real connection string is passed at runtime via --connection.
    New-MigrationsBundle `
        -provider "PostgreSql" `
        -connectionString "Host=127.0.0.1;Port=5432;Database=kitos;Username=postgres;Password=localNoSecret" `
        -outputPath $postgresBundleExe
}
finally {
    Remove-Item Env:Database__Provider -ErrorAction SilentlyContinue
    Remove-Item Env:ConnectionStrings__KitosContext -ErrorAction SilentlyContinue
}

Write-Host "Migrations bundle created at:"
Write-Host "  $postgresBundleExe"