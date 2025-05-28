$ErrorActionPreference = "Stop"

.$PSScriptRoot\PubsubDBMigrations.ps1

#-------------------------------------------------------------
Write-Host "Running migrations"
#-------------------------------------------------------------
Run-Pubsub-DB-Migrations -connectionString "Server=.\SQLEXPRESS;Integrated Security=true;Initial Catalog=Kitos_PubSub;MultipleActiveResultSets=True;Encrypt=True;TrustServerCertificate=True;"