#-------------------------------------------------------------
# Stop on first error
#-------------------------------------------------------------
$ErrorActionPreference = "Stop"

#-------------------------------------------------------------
# Load helper libraries
#-------------------------------------------------------------
.$PSScriptRoot\AwsApi.ps1

Configure-Aws-From-User-Input
Load-Environment-Secrets-From-Aws -envName "integration"

$TestToolsPath = Resolve-Path "$PSScriptRoot\..\TestDatabaseTools\Tools.Test.Database.exe"
$MigrationsFolder = Resolve-Path "$PSScriptRoot\..\DataAccessApp"

#-------------------------------------------------------------
Write-Host "Dropping existing databases (kitos and hangfire)"
#-------------------------------------------------------------

& $TestToolsPath "DropDatabase" "$Env:KitosDbConnectionStringForTeamCity"
if($LASTEXITCODE -ne 0)	{ Throw "FAILED TO LOAD $parameterName from $environmentName" }

& $TestToolsPath "DropDatabase" "$Env:HangfireDbConnectionStringForTeamCity"
if($LASTEXITCODE -ne 0)	{ Throw "FAILED TO LOAD $parameterName from $environmentName" }

#-------------------------------------------------------------
Write-Host "Running migrations"
#-------------------------------------------------------------
$Env:SeedNewDb="yes"
& "$MigrationsFolder\migrate.exe" "Infrastructure.DataAccess.dll" /connectionString="$Env:KitosDbConnectionStringForTeamCity" /connectionProviderName="System.Data.SqlClient" /verbose /startupDirectory="$MigrationsFolder"
if($LASTEXITCODE -ne 0)	{ Throw "FAILED TO LOAD $parameterName from $environmentName" }

#-------------------------------------------------------------
Write-Host "Enabling custom options"
#-------------------------------------------------------------

& $TestToolsPath "EnableAllOptions" "$Env:KitosDbConnectionStringForTeamCity"
if($LASTEXITCODE -ne 0)	{ Throw "FAILED TO LOAD $parameterName from $environmentName" }

#-------------------------------------------------------------
Write-Host "Configuring test users"
#-------------------------------------------------------------

& $TestToolsPath "CreateTestUser" "$Env:KitosDbConnectionStringForTeamCity" "kitos-integration-globaladmin@strongminds.dk" "todoCreateRealOne" "GlobalAdmin"
if($LASTEXITCODE -ne 0)	{ Throw "FAILED TO LOAD $parameterName from $environmentName" }

& $TestToolsPath "CreateTestUser" "$Env:KitosDbConnectionStringForTeamCity" "kitos-integration-localadmin@strongminds.dk" "todoCreateRealOne" "LocalAdmin"
if($LASTEXITCODE -ne 0)	{ Throw "FAILED TO LOAD $parameterName from $environmentName" }

& $TestToolsPath "CreateTestUser" "$Env:KitosDbConnectionStringForTeamCity" "kitos-integration-normal-user@strongminds.dk" "todoCreateRealOne" "User"
if($LASTEXITCODE -ne 0)	{ Throw "FAILED TO LOAD $parameterName from $environmentName" }