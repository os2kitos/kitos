Function Run-DB-Migrations([bool]$newDb = $false, [string]$migrationsFolder, [string]$connectionString) {
    Write-Host "Executing db migrations"

    if($newDb -eq $true) {
        Write-Host "Enabling seed for new database"
        $Env:SeedNewDb="yes"
    } else {
        Write-Host "Disabling seed for new database"
        $Env:SeedNewDb="no"
    }
    
    & "$migrationsFolder\migrate.exe" "Infrastructure.DataAccess.dll" /connectionString="$connectionString" /connectionProviderName="System.Data.SqlClient" /verbose /startupDirectory="$migrationsFolder"
    
    if($LASTEXITCODE -ne 0)	{ Throw "FAILED TO MIGRATE DB" }
}