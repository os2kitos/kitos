#-------------------------------------------------------------
# Stop on first error
#-------------------------------------------------------------
$ErrorActionPreference = "Stop"

#-------------------------------------------------------------
# Load helper libraries
#-------------------------------------------------------------
.$PSScriptRoot\DeploymentSetup.ps1
.$PSScriptRoot\DbMigrations.ps1

Setup-Environment -environmentName "test"

#-------------------------------------------------------------
Write-Host "Running migrations"
#-------------------------------------------------------------
Run-DB-Migrations -newDb $false