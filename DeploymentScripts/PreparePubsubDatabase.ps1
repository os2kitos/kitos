param (
    [Parameter(Mandatory = $true)]
    [string]$targetEnvironment,
    [string]$databaseProvider = "PostgreSql"
)

$ErrorActionPreference = "Stop"

.$PSScriptRoot\SetupPubsubEnviroment.ps1
.$PSScriptRoot\PubsubDBMigrations.ps1

Load-Pubsub-Parameters -envName $targetEnvironment

$connectionString = $Env:PUBSUB_MIGRATION_CONNECTION_STRING

#-------------------------------------------------------------
Write-Host "Running migrations"
#-------------------------------------------------------------
Run-Pubsub-DB-Migrations -connectionString $connectionString -provider $databaseProvider