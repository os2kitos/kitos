Function Run-DB-Migrations([bool]$newDb = $false) {
    Write-Host "Executing db migrations"
    
    $MigrationsFolder = Resolve-Path "$PSScriptRoot\..\DataAccessApp"

    if($newDb -eq $true) {
        Write-Host "Enabling seed for new database"
        $Env:SeedNewDb="yes"
    } else {
        Write-Host "Disabling seed for new database"
        $Env:SeedNewDb="no"
    }
    
    & "$MigrationsFolder\migrate.exe" "Infrastructure.DataAccess.dll" /connectionString="$Env:KitosDbConnectionStringForTeamCity" /connectionProviderName="System.Data.SqlClient" /verbose /startupDirectory="$MigrationsFolder"
    
    if($LASTEXITCODE -ne 0)	{ Throw "FAILED TO MIGRATE DB" }
}