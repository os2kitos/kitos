#-------------------------------------------------------------
# Stop on first error
#-------------------------------------------------------------
$ErrorActionPreference = "Stop"

param(
    [Parameter(Mandatory=$true)][string]$testToolsExePath,
    [Parameter(Mandatory=$true)][string]$migrationsFolderPath,
    [Parameter(Mandatory=$true)][string]$kitosDbConnectionString,
    [Parameter(Mandatory=$true)][string]$hangfireDbConnectionString,
    [Parameter(Mandatory=$true)][string]$globalAdminUserName,
    [Parameter(Mandatory=$true)][string]$globalAdminPw,
    [Parameter(Mandatory=$true)][string]$localAdminUserName,
    [Parameter(Mandatory=$true)][string]$localAdminPw,
    [Parameter(Mandatory=$true)][string]$normalUserUserName,
    [Parameter(Mandatory=$true)][string]$normalUserPw
    )

#-------------------------------------------------------------
# Load helper libraries
#-------------------------------------------------------------
.$PSScriptRoot\DbMigrations.ps1

#-------------------------------------------------------------
Write-Host "Dropping existing databases (kitos and hangfire)"
#-------------------------------------------------------------

& $testToolsExePath "DropDatabase" "$kitosDbConnectionString"
if($LASTEXITCODE -ne 0)	{ Throw "FAILED TO DROP KITOS DB" }

& $testToolsExePath "DropDatabase" "$hangfireDbConnectionString"
if($LASTEXITCODE -ne 0)	{ Throw "FAILED TO DROP HANGFIRE DB" }

#-------------------------------------------------------------
Write-Host "Running migrations"
#-------------------------------------------------------------
Run-DB-Migrations -newDb $true -migrationsFolder "$migrationsFolderPath" -connectionString "$kitosDbConnectionString"

#-------------------------------------------------------------
Write-Host "Enabling custom options"
#-------------------------------------------------------------

& $testToolsExePath "EnableAllOptions" "$kitosDbConnectionString"
if($LASTEXITCODE -ne 0)	{ Throw "FAILED TO ENABLE ALL OPTIONS IN KITOS DB" }

#-------------------------------------------------------------
Write-Host "Configuring test users"
#-------------------------------------------------------------

& $testToolsExePath "CreateTestUser" "$kitosDbConnectionString" "$globalAdminUserName" "$globalAdminPw" "GlobalAdmin"
if($LASTEXITCODE -ne 0)	{ Throw "FAILED TO CREATE GLOBAL ADMIN" }

& $testToolsExePath "CreateTestUser" "$kitosDbConnectionString" "$localAdminUserName" "$localAdminPw" "LocalAdmin"
if($LASTEXITCODE -ne 0)	{ Throw "FAILED TO CREATE LOCAL ADMIN" }

& $testToolsExePath "CreateTestUser" "$kitosDbConnectionString" "$normalUserUserName" "$normalUserPw" "User"
if($LASTEXITCODE -ne 0)	{ Throw "FAILED TO CREATE NORMAL USER" }

#-------------------------------------------------------------
Write-Host "Create IT System"
#-------------------------------------------------------------
& $testToolsExePath "CreateItSystem" "$kitosDbConnectionString" "DefaultTestItSystem"
if($LASTEXITCODE -ne 0)	{ Throw "FAILED TO IT SYSTEM" }

#-------------------------------------------------------------
Write-Host "Create IT Contract"
#-------------------------------------------------------------
& $testToolsExePath "CreateItContract" "$kitosDbConnectionString" "DefaultTestItContract"
if($LASTEXITCODE -ne 0)	{ Throw "FAILED TO IT CONTRACT" }