param (
    [Parameter(Mandatory = $true)]
    [string]$targetEnvironment
)

$ErrorActionPreference = "Stop"

.$PSScriptRoot\SetupPubsubEnviroment.ps1
.$PSScriptRoot\PubsubDBMigrations.ps1

Load-Pubsub-Parameters -envName $targetEnvironment

#-------------------------------------------------------------
Write-Host "Running migrations"
#-------------------------------------------------------------
Run-Pubsub-DB-Migrations -connectionString "$Env:PUBSUB_MIGRATION_CONNECTION_STRING"