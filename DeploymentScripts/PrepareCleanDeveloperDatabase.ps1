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
    [Parameter(Mandatory=$true)][string]$normalUserPw,
    [Parameter(Mandatory=$true)][string]$apiUserUserName,
    [Parameter(Mandatory=$true)][string]$apiUserPw,
    [Parameter(Mandatory=$true)][string]$apiGlobalAdminUserName,
    [Parameter(Mandatory=$true)][string]$apiGlobalAdminPw,
    [Parameter(Mandatory=$true)][string]$systemIntegratorEmail,
    [Parameter(Mandatory=$true)][string]$systemIntegratorPw
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

##-------------------------------------------------------------
Write-Host "Creating test database"
#-------------------------------------------------------------
& $testToolsExePath "CreateCleanTestDatabase"  `
                    "$kitosDbConnectionString" `
                    "$globalAdminUserName" "$globalAdminPw"  `
                    "$localAdminUserName" "$localAdminPw"  `
                    "$normalUserUserName" "$normalUserPw"  `
                    "$apiUserUserName" "$apiUserPw"  `
                    "$apiGlobalAdminUserName" "$apiGlobalAdminPw"  `
                    "$systemIntegratorEmail" "$systemIntegratorPw"

if($LASTEXITCODE -ne 0)	{ Throw "FAILED TO CREATE TEST DATABASE" }