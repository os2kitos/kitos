Function LooksLikePostgreSqlConnectionString([string]$connectionString) {
    if (-not $connectionString) { return $false }
    # PostgreSQL-specific keys
    return (
        $connectionString -imatch 'Host=' -or
        $connectionString -imatch 'Username='
    )
}

Function Get-DatabaseProvider {
    param([string]$connectionString = $null)
    # Connection string format is authoritative: a PostgreSQL-formatted connection string
    # cannot be used with any other provider, so detect from it first.
    if ($connectionString -and (LooksLikePostgreSqlConnectionString $connectionString)) { return "PostgreSql" }
    if ($Env:Database__Provider) { return $Env:Database__Provider }
    return "PostgreSql"
}

Function Is-PostgreSqlProvider([string]$provider) {
    return $provider -and (
        $provider.Equals("PostgreSql", [System.StringComparison]::OrdinalIgnoreCase) -or
        $provider.Equals("Postgres", [System.StringComparison]::OrdinalIgnoreCase) -or
        $provider.Equals("Npgsql", [System.StringComparison]::OrdinalIgnoreCase)
    )
}

Function ConvertTo-PostgresConnectionParts([string]$connectionString) {
    $cs = @{ Host = $null; Server = $null; Port = $null; Database = $null; UserId = $null; Username = $null; Password = $null; "SSL Mode" = $null }
    ($connectionString -split ';') | Where-Object { $_ -match '=' } | ForEach-Object {
        $kv = $_ -split '=', 2
        $cs[$kv[0].Trim()] = $kv[1].Trim()
    }

    $pgHost = if ($cs['Host']) { $cs['Host'] } else { $cs['Server'] }
    $pgPort = if ($cs['Port']) { $cs['Port'] } else { "5432" }
    $pgDatabase = $cs['Database']
    $pgUsername = if ($cs['Username']) { $cs['Username'] } else { $cs['User ID'] }

    return @{
        Host     = $pgHost
        Port     = $pgPort
        Database = $pgDatabase
        Username = $pgUsername
        Password = $cs['Password']
        SslMode  = $cs['SSL Mode']
    }
}

Function Normalize-PostgresConnectionString([string]$connectionString) {
    $parts = ConvertTo-PostgresConnectionParts $connectionString

    if (-not $parts.Database) {
        throw "PostgreSQL connection string must contain Database"
    }

    # PostgreSQL folds unquoted identifiers to lowercase. Keep db names lowercase to avoid
    # runtime mismatches between CREATE DATABASE and subsequent connections.
    $normalizedDatabase = $parts.Database.ToLowerInvariant()
    $normalizedHost = if ($parts.Host -and $parts.Host.Equals("localhost", [System.StringComparison]::OrdinalIgnoreCase)) {
        "127.0.0.1"
    } else {
        $parts.Host
    }

    $sslPart = if ($parts.SslMode) { ";SSL Mode=$($parts.SslMode)" } else { "" }
    return "Host=$normalizedHost;Port=$($parts.Port);Database=$normalizedDatabase;Username=$($parts.Username);Password=$($parts.Password)$sslPart"
}

Function Get-PostgresCliPath {
    $psql = Get-Command psql -ErrorAction SilentlyContinue
    if ($psql) {
        return $psql.Source
    }

    throw "PostgreSQL provider requires psql to be installed and available on PATH."
}

Function Set-PostgresCliEnvironment([hashtable]$parts) {
    $state = @{
        HadPgPassword = Test-Path Env:PGPASSWORD
        PgPassword    = $Env:PGPASSWORD
        HadPgSslMode  = Test-Path Env:PGSSLMODE
        PgSslMode     = $Env:PGSSLMODE
        SetPgSslMode  = $parts.SslMode -and $parts.SslMode.Equals("Disable", [System.StringComparison]::OrdinalIgnoreCase)
    }

    $Env:PGPASSWORD = $parts.Password
    if ($state.SetPgSslMode) {
        $Env:PGSSLMODE = "disable"
    }

    return $state
}

Function Restore-PostgresCliEnvironment([hashtable]$state) {
    if ($state.HadPgPassword) {
        $Env:PGPASSWORD = $state.PgPassword
    } else {
        Remove-Item Env:PGPASSWORD -ErrorAction SilentlyContinue
    }

    if ($state.SetPgSslMode) {
        if ($state.HadPgSslMode) {
            $Env:PGSSLMODE = $state.PgSslMode
        } else {
            Remove-Item Env:PGSSLMODE -ErrorAction SilentlyContinue
        }
    }
}

Function Invoke-PostgresSql([hashtable]$parts, [string]$database, [string]$sql) {
    $psqlPath = Get-PostgresCliPath
    $postgresCliEnvironment = Set-PostgresCliEnvironment -parts $parts
    try {
        & $psqlPath -h $parts.Host -p $parts.Port -U $parts.Username -d $database -v ON_ERROR_STOP=1 -c $sql
        if ($LASTEXITCODE -ne 0) { throw "psql failed executing SQL" }
    } finally {
        Restore-PostgresCliEnvironment -state $postgresCliEnvironment
    }
}

Function Invoke-PostgresSqlFileInternal([hashtable]$parts, [string]$sqlFilePath) {
    $normalizedSqlPath = Get-NormalizedPostgresSqlFile -sqlFilePath $sqlFilePath
    $psqlPath = Get-PostgresCliPath
    $postgresCliEnvironment = Set-PostgresCliEnvironment -parts $parts
    try {
        & $psqlPath -h $parts.Host -p $parts.Port -U $parts.Username -d $parts.Database -v ON_ERROR_STOP=1 -f $normalizedSqlPath
        if ($LASTEXITCODE -ne 0) { throw "psql failed executing $normalizedSqlPath" }
    } finally {
        Restore-PostgresCliEnvironment -state $postgresCliEnvironment
        if ($normalizedSqlPath -ne $sqlFilePath) {
            Remove-Item -Path $normalizedSqlPath -ErrorAction SilentlyContinue
        }
    }
}

Function Get-NormalizedPostgresSqlFile([string]$sqlFilePath) {
    $content = Get-Content -Path $sqlFilePath -Raw
    $lines = $content -split "`r?`n"
    $seenIndexNames = @{}
    $usedFinalIndexNames = @{}

    for ($i = 0; $i -lt $lines.Length; $i++) {
        $line = $lines[$i]
        $match = [regex]::Match($line, '^(CREATE\s+(?:UNIQUE\s+)?INDEX\s+")([^"]+)("\s+ON\s+")([^"]+)("\s*\()', [System.Text.RegularExpressions.RegexOptions]::IgnoreCase)
        if (-not $match.Success) { continue }

        $indexName = $match.Groups[2].Value
        $tableName = $match.Groups[4].Value

        if (-not $seenIndexNames.ContainsKey($indexName)) {
            $seenIndexNames[$indexName] = 1
            $finalName = Get-UniquePostgresIdentifier -candidateName $indexName -usedNames $usedFinalIndexNames
            $usedFinalIndexNames[$finalName] = $true
            if ($finalName -ne $indexName) {
                $lines[$i] = $line.Substring(0, $match.Groups[2].Index) + $finalName + $line.Substring($match.Groups[2].Index + $match.Groups[2].Length)
            }
            continue
        }

        $seenIndexNames[$indexName] = $seenIndexNames[$indexName] + 1
        $suffix = "_${tableName}_$($seenIndexNames[$indexName])"
        $newIndexName = Get-UniquePostgresIdentifier -candidateName "$indexName$suffix" -usedNames $usedFinalIndexNames
        $usedFinalIndexNames[$newIndexName] = $true

        $lines[$i] = $line.Substring(0, $match.Groups[2].Index) + $newIndexName + $line.Substring($match.Groups[2].Index + $match.Groups[2].Length)
    }

    $normalized = [string]::Join([Environment]::NewLine, $lines)
    if ($normalized -eq $content) {
        return $sqlFilePath
    }

    $tmpFile = [System.IO.Path]::GetTempFileName() -replace '\.tmp$', '.sql'
    $utf8NoBom = New-Object System.Text.UTF8Encoding($false)
    [System.IO.File]::WriteAllText($tmpFile, $normalized, $utf8NoBom)
    return $tmpFile
}

Function Get-UniquePostgresIdentifier([string]$candidateName, [hashtable]$usedNames) {
    $maxLength = 63
    $base = if ($candidateName.Length -gt $maxLength) { $candidateName.Substring(0, $maxLength) } else { $candidateName }

    if (-not $usedNames.ContainsKey($base)) {
        return $base
    }

    $counter = 2
    while ($true) {
        $suffix = "_$counter"
        $prefixLength = $maxLength - $suffix.Length
        $prefix = if ($candidateName.Length -gt $prefixLength) { $candidateName.Substring(0, $prefixLength) } else { $candidateName }
        $variant = "$prefix$suffix"
        if (-not $usedNames.ContainsKey($variant)) {
            return $variant
        }
        $counter++
    }
}

Function New-PostgresDatabase([string]$connectionString) {
    $parts = ConvertTo-PostgresConnectionParts $connectionString
    if (-not $parts.Database) { throw "PostgreSQL connection string must contain Database" }

    $escapedDatabaseNameForIdentifier = $parts.Database.Replace('"', '""')
    $escapedDatabaseNameForLiteral = $parts.Database.Replace("'", "''")
    $existsQuery = "SELECT 1 FROM pg_database WHERE datname = '$escapedDatabaseNameForLiteral'"
    $createDatabaseSql = "CREATE DATABASE `"$escapedDatabaseNameForIdentifier`""
    $psqlPath = Get-PostgresCliPath

    $postgresCliEnvironment = Set-PostgresCliEnvironment -parts $parts
    try {
        $existsOutput = (& $psqlPath -h $parts.Host -p $parts.Port -U $parts.Username -d postgres -tAc $existsQuery | Out-String)
        if ($LASTEXITCODE -ne 0) { throw "psql failed checking whether database $($parts.Database) exists" }

        $exists = if ($existsOutput) { $existsOutput.Trim() } else { "" }
        if ($exists -ne "1") {
            $createDatabaseSql | & $psqlPath -h $parts.Host -p $parts.Port -U $parts.Username -d postgres -v ON_ERROR_STOP=1
            if ($LASTEXITCODE -ne 0) { throw "psql failed creating database $($parts.Database)" }
        }
    } finally {
        Restore-PostgresCliEnvironment -state $postgresCliEnvironment
    }
}

Function Wait-ForTcpPort {
    param(
        [Parameter(Mandatory = $true)][string]$Hostname,
        [Parameter(Mandatory = $true)][int]$Port,
        [int]$MaxAttempts = 12,
        [int]$DelaySeconds = 5
    )

    for ($i = 1; $i -le $MaxAttempts; $i++) {
        Write-Host "Checking TCP connectivity to $Hostname`:$Port (attempt $i/$MaxAttempts) ..."

        $ok = Test-NetConnection -ComputerName $Hostname -Port $Port -InformationLevel Quiet -WarningAction SilentlyContinue

        if ($ok) {
            Write-Host "TCP connectivity OK ($Hostname`:$Port)"
            return
        }

        if ($i -lt $MaxAttempts) {
            Start-Sleep -Seconds $DelaySeconds
        }
    }

    throw "TCP connectivity was not established to $Hostname`:$Port after $MaxAttempts attempts."
}

# Ensures the named PostgreSQL role exists, creating it with CREATEDB if it does not.
# The role password defaults to the role name itself (matching the Docker init-databases.sh
# convention) but can be overridden via the KITOS_APP_PASSWORD environment variable.
Function Ensure-PostgresRole([hashtable]$parts, [string]$roleName) {
    if ([string]::IsNullOrWhiteSpace($roleName)) { return }

    $rolePassword = if ($Env:KITOS_APP_PASSWORD) { $Env:KITOS_APP_PASSWORD } else { $roleName }
    $escapedRoleName = $roleName.Replace("'", "''").Replace('"', '""')
    $escapedPassword = $rolePassword.Replace("'", "''")
    Write-Host "Ensuring PostgreSQL role '$roleName' exists"
    $sql = @"
DO `$`$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM pg_roles WHERE rolname = '$escapedRoleName') THEN
        EXECUTE 'CREATE ROLE "$escapedRoleName" WITH LOGIN PASSWORD ''$escapedPassword'' CREATEDB';
    END IF;
END
`$`$;
"@
    Invoke-PostgresSql -parts $parts -database "postgres" -sql $sql
}

# Grants full access on a schema and all its objects to the named user.
Function Grant-PostgresSchemaPrivileges([hashtable]$parts, [string]$granteeUser, [string]$schemaName) {
    if ([string]::IsNullOrWhiteSpace($granteeUser)) { return }
    if ([string]::IsNullOrWhiteSpace($schemaName)) { return }

    Write-Host "Granting $schemaName schema privileges to '$granteeUser'"
    $escapedUsername = $granteeUser.Replace("'", "''").Replace('"', '""')
    $escapedDatabaseName = $parts.Database.Replace("'", "''").Replace('"', '""')
    $escapedSchemaName = $schemaName.Replace("'", "''").Replace('"', '""')
    $sql = @"
DO `$`$
BEGIN
    IF EXISTS (SELECT 1 FROM pg_roles WHERE rolname = '$escapedUsername') THEN
        EXECUTE 'GRANT CONNECT, TEMPORARY, CREATE ON DATABASE "$escapedDatabaseName" TO "$escapedUsername"';
        IF EXISTS (SELECT 1 FROM information_schema.schemata WHERE schema_name = '$escapedSchemaName') THEN
            EXECUTE 'GRANT USAGE, CREATE ON SCHEMA "$escapedSchemaName" TO "$escapedUsername"';
            EXECUTE 'GRANT ALL PRIVILEGES ON ALL TABLES IN SCHEMA "$escapedSchemaName" TO "$escapedUsername"';
            EXECUTE 'GRANT ALL PRIVILEGES ON ALL SEQUENCES IN SCHEMA "$escapedSchemaName" TO "$escapedUsername"';
            EXECUTE 'ALTER DEFAULT PRIVILEGES IN SCHEMA "$escapedSchemaName" GRANT ALL ON TABLES TO "$escapedUsername"';
            EXECUTE 'ALTER DEFAULT PRIVILEGES IN SCHEMA "$escapedSchemaName" GRANT ALL ON SEQUENCES TO "$escapedUsername"';
        END IF;
    END IF;
END
`$`$;
"@
    Invoke-PostgresSql -parts $parts -database $parts.Database -sql $sql
}

Function Run-DB-Migrations([bool]$newDb = $false, [string]$connectionString, [string]$buildConfiguration = "Release") {
    Write-Host "Executing db migrations"
    $connectionString = Normalize-PostgresConnectionString -connectionString $connectionString
    $pgParts = ConvertTo-PostgresConnectionParts $connectionString
    Write-Host "Using PostgreSQL connection target Host=$($pgParts.Host);Database=$($pgParts.Database)"

    if ($newDb -eq $true) {
        Write-Host "Enabling seed for new database"
        $Env:SeedNewDb = "yes"
    } else {
        Write-Host "Disabling seed for new database"
        $Env:SeedNewDb = "no"
    }

    # Verify TCP connectivity before proceeding with any migration operations.
    $pgHost = $pgParts.Host.Trim()
    $isLocalServer = $newDb -eq $true -or ($pgHost -match '^(\.|(\(local\))|localhost|(\(localdb\)))(\\|,|$)')
    if (-not $isLocalServer) {
        $pgPort = if ($pgParts.Port) { [int]$pgParts.Port } else { 5432 }
        Wait-ForTcpPort -Hostname $pgHost -Port $pgPort
    }

    $repoRoot = Resolve-Path "$PSScriptRoot\.."
    $infraProject = "$repoRoot\Infrastructure.DataAccess\Infrastructure.DataAccess.csproj"
    # Use Infrastructure.DataAccess as startup for dotnet ef fallback.
    # This avoids loading Presentation.Web (not required because KitosContextDesignTimeFactory
    # resolves provider/connection from environment variables).
    $startupProject = $infraProject

    Write-Host "Ensuring PostgreSQL database exists"
    New-PostgresDatabase -connectionString $connectionString

    # Expose the connection string via the standard .NET env var so the
    # KitosContextDesignTimeFactory can pick it up without a hardcoded fallback.
    $Env:ConnectionStrings__KitosContext = $connectionString
    $Env:Database__Provider = "PostgreSql"
    $Env:IgnorePendingModelChangesWarning = "true"

    # CI path: use pre-built self-contained bundle (no source or SDK required on the agent).
    # Local dev fallback: build and run via dotnet ef when the bundle is not present.
    $bundleExe = "$PSScriptRoot\..\MigrationsBundle\efbundle.postgresql.exe"

    $preferBundle = $buildConfiguration -eq "Release" -or $Env:UseMigrationsBundle -eq "true"
    $useBundle = $preferBundle -and (Test-Path $bundleExe)
    if ($useBundle) {
        $migrationInputPaths = @(
            "$repoRoot\Infrastructure.DataAccess\KitosContext.cs",
            "$repoRoot\Infrastructure.DataAccess\KitosContextDesignTimeFactory.cs",
            "$repoRoot\Infrastructure.DataAccess\KitosNpgsqlMigrationsSqlGenerator.cs"
        )
        $migrationFiles = Get-ChildItem -Path "$repoRoot\Infrastructure.DataAccess\Migrations\EfCore" -Filter "*.cs" -File -Recurse -ErrorAction SilentlyContinue
        $migrationInputPaths += $migrationFiles | ForEach-Object { $_.FullName }

        $newestInputUtc = [DateTime]::MinValue
        foreach ($path in $migrationInputPaths) {
            if (-not (Test-Path -LiteralPath $path)) {
                continue
            }

            $lastWrite = (Get-Item -LiteralPath $path).LastWriteTimeUtc
            if ($lastWrite -gt $newestInputUtc) {
                $newestInputUtc = $lastWrite
            }
        }

        $bundleTimestampUtc = (Get-Item -LiteralPath $bundleExe).LastWriteTimeUtc
        if ($bundleTimestampUtc -lt $newestInputUtc) {
            Write-Warning "Skipping stale migrations bundle at $bundleExe. Newer migration inputs exist ($newestInputUtc). Falling back to dotnet ef."
            $useBundle = $false
        }
    }

    if ($useBundle) {
        Write-Host "Using pre-built migrations bundle at $bundleExe"
        $bundleArgs = @("--connection", "$connectionString")
        $verboseBundleLogging = $Env:VerboseMigrationLogging -eq "true"
        if ($verboseBundleLogging) {
            $bundleArgs += "--verbose"
        }
        $bundleOutput = & "$bundleExe" @bundleArgs 2>&1
        $bundleExitCode = $LASTEXITCODE

        if ($verboseBundleLogging) {
            $bundleOutput | ForEach-Object { Write-Host $_ }
        }
        else {
            $skipHistoryProbeLines = 0
            foreach ($line in $bundleOutput) {
                $text = "$line"

                if ($skipHistoryProbeLines -gt 0) {
                    $skipHistoryProbeLines--
                    continue
                }

                if ($text -match '^Failed executing DbCommand .*' -and $newDb) {
                    $skipHistoryProbeLines = 3
                    continue
                }

                Write-Host $text
            }
        }

        if ($bundleExitCode -ne 0) { Throw "FAILED TO MIGRATE DB" }
    }

    if (-not $useBundle) {
        if (-not $preferBundle) {
            Write-Host "Running dotnet ef database update (bundle disabled for this configuration)"
        } else {
            Write-Host "Migrations bundle unavailable or stale, running dotnet ef database update"
        }
        dotnet ef database update `
            --project "$infraProject" `
            --startup-project "$startupProject" `
            --connection "$connectionString" `
            --configuration "$buildConfiguration"

        if ($LASTEXITCODE -ne 0) { Throw "FAILED TO MIGRATE DB" }
    }

    # When the script runs as a superuser (e.g. postgres) but the application connects as a
    # different user (e.g. kitos in Docker), the dbo schema ends up owned by the superuser.
    # Grant the known app user access so the running applications are not blocked.
    # Must run after migrations so the target schema already exists.
    # This is a no-op when the script already runs as the app user (kitos owns the schema).
    if ($newDb -eq $true) {
        $knownAppUser = if ($Env:KITOS_APP_USER) { $Env:KITOS_APP_USER } else { "kitos" }
        if ($pgParts.Username -ne $knownAppUser) {
            Grant-PostgresSchemaPrivileges -parts $pgParts -granteeUser $knownAppUser -schemaName "dbo"
            Grant-PostgresSchemaPrivileges -parts $pgParts -granteeUser $knownAppUser -schemaName "public"
        }
    }
}
