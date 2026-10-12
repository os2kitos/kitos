param(
    [string]$pubsubDbConnectionString = "Host=127.0.0.1;Port=5432;Database=kitos_pubsub;Username=kitos;Password=kitos",
    [switch]$ignorePendingModelChangesWarning
)

$ErrorActionPreference = "Stop"

.$PSScriptRoot\PubsubDBMigrations.ps1

$previousDatabaseProvider = $Env:Database__Provider
$previousIgnorePendingModelChangesWarning = $Env:IgnorePendingModelChangesWarning

try {
    $Env:Database__Provider = "PostgreSql"

    if ($ignorePendingModelChangesWarning) {
        $Env:IgnorePendingModelChangesWarning = "true"
    } else {
        Remove-Item Env:IgnorePendingModelChangesWarning -ErrorAction SilentlyContinue
    }

    #-------------------------------------------------------------
    Write-Host "Running PubSub migrations on local PostgreSQL"
    #-------------------------------------------------------------
    Run-Pubsub-DB-Migrations -connectionString $pubsubDbConnectionString
}
finally {
    if ($null -eq $previousDatabaseProvider) {
        Remove-Item Env:Database__Provider -ErrorAction SilentlyContinue
    }
    else {
        $Env:Database__Provider = $previousDatabaseProvider
    }

    if ($null -eq $previousIgnorePendingModelChangesWarning) {
        Remove-Item Env:IgnorePendingModelChangesWarning -ErrorAction SilentlyContinue
    }
    else {
        $Env:IgnorePendingModelChangesWarning = $previousIgnorePendingModelChangesWarning
    }
}
