$ErrorActionPreference = "Stop"

.$PSScriptRoot\PubsubDBMigrations.ps1

#-------------------------------------------------------------
Write-Host "Running migrations"
#-------------------------------------------------------------
Run-Pubsub-DB-Migrations -connectionString "Host=localhost;Port=5432;Database=Kitos_PubSub;Username=postgres;******"
