.$PSScriptRoot\DbMigrations.ps1

Function Run-Pubsub-DB-Migrations(
    [string]$connectionString,
    [string]$provider = "PostgreSql"
) {
    $dataAccessFolder = Resolve-Path "$PSScriptRoot\..\PubSub.Infrastructure.DataAccess"

    # Refresh restore metadata and packages in the account running migrations.
    Write-Host "Restoring PubSub database migration dependencies"
    & dotnet restore "$dataAccessFolder\PubSub.Infrastructure.DataAccess.csproj" --force --no-cache
    if ($LASTEXITCODE -ne 0) {
        Throw "PubSub package restore failed with exit code $LASTEXITCODE."
    }

    if (Is-PostgreSqlProvider $provider) {
        $connectionString = Normalize-PostgresConnectionString -connectionString $connectionString
        New-PostgresDatabase -connectionString $connectionString
    }

    # Set the environment variables for the design-time factory
    $env:DEFAULT_CONNECTION_STRING = $connectionString
    $env:Database__Provider = $provider

    & dotnet ef database update --project "$dataAccessFolder" --connection "$connectionString"

    # Check for errors
    if ($LASTEXITCODE -ne 0) { 
        Write-Error "Migration failed with exit code $LASTEXITCODE."
        Throw "FAILED TO MIGRATE PUBSUB DB" 
    }
}
