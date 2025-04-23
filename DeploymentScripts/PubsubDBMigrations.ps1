Function Run-Pubsub-DB-Migrations(
    [string]$connectionString
) {
    $dataAccessFolder = Resolve-Path "$PSScriptRoot\..\PubSub.DataAccess"

    # Set the environment variable for the design-time factory
    $env:DEFAULT_CONNECTION_STRING = $connectionString

    & dotnet ef database update --project "$dataAccessFolder" --connection "$connectionString"

    # Check for errors
    if ($LASTEXITCODE -ne 0) { 
        Write-Error "Migration failed with exit code $LASTEXITCODE."
        Throw "FAILED TO MIGRATE PUBSUB DB" 
    }
}
