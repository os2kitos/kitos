param(
    [Parameter(Mandatory=$true)][string]$testToolsExePath,
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
    [Parameter(Mandatory=$true)][string]$systemIntegratorPw,
    [string]$buildConfiguration = "Debug",
    [switch]$stopWebHostDuringReset
    )
    
#-------------------------------------------------------------
# Stop on first error
#-------------------------------------------------------------
$ErrorActionPreference = "Stop"

#-------------------------------------------------------------
# Load helper libraries
#-------------------------------------------------------------
.$PSScriptRoot\DbMigrations.ps1

if (-not $Env:Database__Provider -and $Env:KitosDbProvider) {
    $Env:Database__Provider = $Env:KitosDbProvider
}

$databaseProvider = Get-DatabaseProvider -connectionString $kitosDbConnectionString
if (Is-PostgreSqlProvider $databaseProvider) {
    $kitosDbConnectionString = Normalize-PostgresConnectionString -connectionString $kitosDbConnectionString
    $hangfireDbConnectionString = Normalize-PostgresConnectionString -connectionString $hangfireDbConnectionString
}

$Env:Database__Provider = $databaseProvider
$Env:ConnectionStrings__KitosContext = $kitosDbConnectionString

function Stop-WebHostForDatabaseReset {
    $state = @{
        W3SvcWasRunning = $false
        W3SvcStopped = $false
    }

    $w3svc = Get-Service -Name "W3SVC" -ErrorAction SilentlyContinue
    if ($null -eq $w3svc) {
        Write-Host "W3SVC service not found. Skipping web host shutdown."
        return $state
    }

    if ($w3svc.Status -eq "Running") {
        try {
            Write-Host "Stopping W3SVC to prevent Hangfire reconnects during database drop"
            Stop-Service -Name "W3SVC" -Force -ErrorAction Stop
            $state.W3SvcWasRunning = $true
            $state.W3SvcStopped = $true
            Write-Host "Stopped W3SVC"
        }
        catch {
            Write-Warning "Failed to stop W3SVC: $($_.Exception.Message). Continuing without shutdown."
        }
    }
    else {
        Write-Host "W3SVC is already stopped"
    }

    return $state
}

function Start-WebHostAfterDatabaseReset($state) {
    if ($null -eq $state) {
        return
    }

    if ($state.W3SvcStopped -and $state.W3SvcWasRunning) {
        try {
            Write-Host "Starting W3SVC after database reset"
            Start-Service -Name "W3SVC" -ErrorAction Stop
            Write-Host "Started W3SVC"
        }
        catch {
            Write-Warning "Failed to start W3SVC: $($_.Exception.Message)"
        }
    }
}

#-------------------------------------------------------------
Write-Host "Dropping existing databases (kitos and hangfire)"
#-------------------------------------------------------------

$webHostState = $null
if ($stopWebHostDuringReset) {
    $webHostState = Stop-WebHostForDatabaseReset
}

try {
    & $testToolsExePath "DropDatabase" "$kitosDbConnectionString"
    if($LASTEXITCODE -ne 0) { Throw "FAILED TO DROP KITOS DB" }

    & $testToolsExePath "DropDatabase" "$hangfireDbConnectionString"
    if($LASTEXITCODE -ne 0) { Throw "FAILED TO DROP HANGFIRE DB" }

    if (Is-PostgreSqlProvider $databaseProvider) {
        Write-Host "Ensuring Hangfire PostgreSQL database exists after reset"
        New-PostgresDatabase -connectionString "$hangfireDbConnectionString"

        $hangfireParts = ConvertTo-PostgresConnectionParts $hangfireDbConnectionString
        $knownAppUser = if ($Env:KITOS_APP_USER) { $Env:KITOS_APP_USER } else { "kitos" }
        # Always ensure role and grant privileges regardless of username match:
        # New-PostgresDatabase creates the DB as a superuser, so the app user needs
        # explicit grants even when the connection string username matches the app user.
        Ensure-PostgresRole -parts $hangfireParts -roleName $knownAppUser
        Grant-PostgresSchemaPrivileges -parts $hangfireParts -granteeUser $knownAppUser -schemaName "hangfire"
        Grant-PostgresSchemaPrivileges -parts $hangfireParts -granteeUser $knownAppUser -schemaName "public"
    }
}
finally {
    if ($stopWebHostDuringReset) {
        Start-WebHostAfterDatabaseReset -state $webHostState
    }
}

#-------------------------------------------------------------
Write-Host "Running migrations"
#-------------------------------------------------------------
Run-DB-Migrations -newDb $true -connectionString "$kitosDbConnectionString" -buildConfiguration $buildConfiguration

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