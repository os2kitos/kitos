#-------------------------------------------------------------
# Stop on first error
#-------------------------------------------------------------
$ErrorActionPreference = "Stop"

#-------------------------------------------------------------
# Load helper libraries
#-------------------------------------------------------------
.$PSScriptRoot\DeploymentSetup.ps1
.$PSScriptRoot\DbMigrations.ps1

Setup-Environment -environmentName "production"

#-------------------------------------------------------------
Write-Host "Running migrations"
Write-Host "Migrations folder: $Env:MigrationsFolder"
Write-Host "Connection string: $Env:KitosDbConnectionStringForTeamCity"
#-------------------------------------------------------------
Run-DB-Migrations -newDb $false -migrationsFolder "$Env:MigrationsFolder" -connectionString "$Env:KitosDbConnectionStringForTeamCity"