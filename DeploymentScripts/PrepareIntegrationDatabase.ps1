#-------------------------------------------------------------
# Stop on first error
#-------------------------------------------------------------
$ErrorActionPreference = "Stop"

#-------------------------------------------------------------
# Load helper libraries
#-------------------------------------------------------------
.$PSScriptRoot\DeploymentSetup.ps1
.$PSScriptRoot\DbMigrations.ps1

Setup-Environment -environmentName "integration"

$TestToolsPath = Resolve-Path "$PSScriptRoot\..\TestDatabaseTools\Tools.Test.Database.exe"

#-------------------------------------------------------------
Write-Host "Dropping existing databases (kitos and hangfire)"
#-------------------------------------------------------------

& $TestToolsPath "DropDatabase" "$Env:KitosDbConnectionStringForTeamCity"
if($LASTEXITCODE -ne 0)	{ Throw "FAILED TO DROP KITOS DB" }

& $TestToolsPath "DropDatabase" "$Env:HangfireDbConnectionStringForTeamCity"
if($LASTEXITCODE -ne 0)	{ Throw "FAILED TO DROP HANGFIRE DB" }

#-------------------------------------------------------------
Write-Host "Running migrations"
#-------------------------------------------------------------
Run-DB-Migrations -newDb $true

#-------------------------------------------------------------
Write-Host "Enabling custom options"
#-------------------------------------------------------------

& $TestToolsPath "EnableAllOptions" "$Env:KitosDbConnectionStringForTeamCity"
if($LASTEXITCODE -ne 0)	{ Throw "FAILED TO ENABLE ALL OPTIONS IN KITOS DB" }

#-------------------------------------------------------------
Write-Host "Configuring test users"
#-------------------------------------------------------------

& $TestToolsPath "CreateTestUser" "$Env:KitosDbConnectionStringForTeamCity" "$Env:TestUserGlobalAdmin" "$Env:TestUserGlobalAdminPw" "GlobalAdmin"
if($LASTEXITCODE -ne 0)	{ Throw "FAILED TO CREATE GLOBAL ADMIN" }

& $TestToolsPath "CreateTestUser" "$Env:KitosDbConnectionStringForTeamCity" "$Env:TestUserLocalAdmin" "$Env:TestUserLocalAdminPw" "LocalAdmin"
if($LASTEXITCODE -ne 0)	{ Throw "FAILED TO CREATE LOCAL ADMIN" }

& $TestToolsPath "CreateTestUser" "$Env:KitosDbConnectionStringForTeamCity" "$Env:TestUserNormalUser" "$Env:TestUserNormalUserPw" "User"
if($LASTEXITCODE -ne 0)	{ Throw "FAILED TO CREATE NORMAL USER" }

#-------------------------------------------------------------
Write-Host "Create IT System"
#-------------------------------------------------------------
& $TestToolsPath "CreateItSystem" "$Env:KitosDbConnectionStringForTeamCity" "DefaultTestItSystem"
if($LASTEXITCODE -ne 0)	{ Throw "FAILED TO IT SYSTEM" }

#-------------------------------------------------------------
Write-Host "Create IT Contract"
#-------------------------------------------------------------
& $TestToolsPath "CreateItContract" "$Env:KitosDbConnectionStringForTeamCity" "DefaultTestItContract"
if($LASTEXITCODE -ne 0)	{ Throw "FAILED TO IT CONTRACT" }