param(
    [Parameter(Mandatory=$true)][string]$testToolsExePath,
    [Parameter(Mandatory=$true)][string]$migrationsFolderPath,
    [Parameter(Mandatory=$true)][string]$kitosDbConnectionString,
    [Parameter(Mandatory=$true)][string]$hangfireDbConnectionString,
    [Parameter(Mandatory=$true)][string]$defaultOrganization,
    [Parameter(Mandatory=$true)][string]$secondOrganization,
    [Parameter(Mandatory=$true)][string]$globalAdminUserName,
    [Parameter(Mandatory=$true)][string]$globalAdminPw,
    [Parameter(Mandatory=$true)][string]$localAdminUserName,
    [Parameter(Mandatory=$true)][string]$localAdminPw,
    [Parameter(Mandatory=$true)][string]$normalUserUserName,
    [Parameter(Mandatory=$true)][string]$normalUserPw,
    [Parameter(Mandatory=$true)][string]$apiUserUserName,
    [Parameter(Mandatory=$true)][string]$apiUserPw,
    [Parameter(Mandatory=$true)][string]$apiGlobalAdminUserName,
    [Parameter(Mandatory=$true)][string]$apiGlobalAdminPw
    )
    
#-------------------------------------------------------------
# Stop on first error
#-------------------------------------------------------------
$ErrorActionPreference = "Stop"

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
Write-Host "Configuring test organizations"
#-------------------------------------------------------------

& $testToolsExePath "CreateOrganization" "$kitosDbConnectionString" "1" "$secondOrganization"
if($LASTEXITCODE -ne 0)	{ Throw "FAILED TO CREATE ORGANIZATION" }

#-------------------------------------------------------------
Write-Host "Configuring test users"
#-------------------------------------------------------------

& $testToolsExePath "CreateTestUser" "$kitosDbConnectionString" "$globalAdminUserName" "$globalAdminPw" "GlobalAdmin" "$defaultOrganization"
if($LASTEXITCODE -ne 0)	{ Throw "FAILED TO CREATE GLOBAL ADMIN" }

& $testToolsExePath "CreateTestUser" "$kitosDbConnectionString" "$localAdminUserName" "$localAdminPw" "LocalAdmin" "$defaultOrganization"
if($LASTEXITCODE -ne 0)	{ Throw "FAILED TO CREATE LOCAL ADMIN" }

& $testToolsExePath "CreateTestUser" "$kitosDbConnectionString" "$normalUserUserName" "$normalUserPw" "User" "$defaultOrganization"
if($LASTEXITCODE -ne 0)	{ Throw "FAILED TO CREATE NORMAL USER" }

& $testToolsExePath "CreateApiTestUser" "$kitosDbConnectionString" "$apiUserUserName" "$apiUserPw" "User" "$defaultOrganization"
if($LASTEXITCODE -ne 0)	{ Throw "FAILED TO CREATE APIACCESS USER" }

& $testToolsExePath "CreateApiTestUser" "$kitosDbConnectionString" "$apiGlobalAdminUserName" "$apiGlobalAdminPw" "GlobalAdmin" "$defaultOrganization,$secondOrganization"
if($LASTEXITCODE -ne 0)	{ Throw "FAILED TO CREATE MULTI ORGANIZATION APIACCESS USER" }

#-------------------------------------------------------------
Write-Host "Create IT System"
#-------------------------------------------------------------
& $testToolsExePath "CreateItSystem" "$kitosDbConnectionString" "DefaultTestItSystem" "$defaultOrganization"
if($LASTEXITCODE -ne 0)	{ Throw "FAILED TO CREATE IT SYSTEM" }

& $testToolsExePath "CreateItSystem" "$kitosDbConnectionString" "SecondOrganizationDefaultTestItSystem" "$secondOrganization" "1"
if($LASTEXITCODE -ne 0)	{ Throw "FAILED TO CREATE IT SYSTEM" }

#-------------------------------------------------------------
Write-Host "Create IT Contract"
#-------------------------------------------------------------
& $testToolsExePath "CreateItContract" "$kitosDbConnectionString" "DefaultTestItContract"
if($LASTEXITCODE -ne 0)	{ Throw "FAILED TO CREATE IT CONTRACT" }