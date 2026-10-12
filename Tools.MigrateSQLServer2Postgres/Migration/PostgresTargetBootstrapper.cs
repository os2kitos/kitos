using System.Diagnostics;
using Tools.MigrateSQLServer2Postgres.Cli;

namespace Tools.MigrateSQLServer2Postgres.Migration;

internal sealed class PostgresTargetBootstrapper
{
    private const string PostgresProviderName = "PostgreSql";

    public async Task BootstrapFreshDatabaseAsync(string targetConnectionString)
    {
        var repositoryLayout = RepositoryLayoutLocator.Find();

        CliConsole.Info("Applying PostgreSQL EF Core migrations from an empty target.");
        await ApplyPendingEfCoreMigrationsAsync(targetConnectionString, repositoryLayout);
    }

    private static async Task ApplyPendingEfCoreMigrationsAsync(string targetConnectionString, RepositoryLayout repositoryLayout)
    {
        var environment = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["ConnectionStrings__KitosContext"] = targetConnectionString,
            ["Database__Provider"] = PostgresProviderName,
            ["IgnorePendingModelChangesWarning"] = "true"
        };

        var bundleStatus = GetPostgresBundleStatus(repositoryLayout);
        if (bundleStatus.CanUseBundle)
        {
            CliConsole.Info($"Using up-to-date PostgreSQL migrations bundle {bundleStatus.BundlePath}.");
            await RunProcessAsync(
                fileName: bundleStatus.BundlePath!,
                arguments: ["--connection", targetConnectionString],
                workingDirectory: repositoryLayout.RootPath,
                environment: environment,
                failureMessage: "The PostgreSQL migrations bundle failed while applying migrations.");
            return;
        }

        if (bundleStatus.BundlePath is not null)
        {
            CliConsole.Warning(
                $"Skipping stale PostgreSQL migrations bundle {bundleStatus.BundlePath}. " +
                $"Latest migration input is newer: {bundleStatus.NewestInputPath} ({bundleStatus.NewestInputTimestampUtc:O}) > bundle ({bundleStatus.BundleTimestampUtc:O}).");
        }
        else
        {
            CliConsole.Warning("No PostgreSQL migrations bundle was found. Falling back to dotnet ef against the current repository.");
        }

        CliConsole.Info("Running dotnet ef against the current repository so the latest migrations are applied.");
        await RunProcessAsync(
            fileName: "dotnet",
            arguments:
            [
                "ef",
                "database",
                "update",
                "--project",
                repositoryLayout.InfrastructureProjectPath,
                "--startup-project",
                repositoryLayout.StartupProjectPath,
                "--connection",
                targetConnectionString
            ],
            workingDirectory: repositoryLayout.RootPath,
            environment: environment,
            failureMessage: BuildDotnetEfFailureMessage(bundleStatus));
    }

    private static async Task RunProcessAsync(
        string fileName,
        IReadOnlyList<string> arguments,
        string workingDirectory,
        IReadOnlyDictionary<string, string> environment,
        string failureMessage)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = fileName,
            WorkingDirectory = workingDirectory,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };

        foreach (var argument in arguments)
        {
            startInfo.ArgumentList.Add(argument);
        }

        foreach (var variable in environment)
        {
            startInfo.Environment[variable.Key] = variable.Value;
        }

        using var process = Process.Start(startInfo)
            ?? throw new InvalidOperationException($"Failed to start process '{fileName}'.");

        var stdOutTask = process.StandardOutput.ReadToEndAsync();
        var stdErrTask = process.StandardError.ReadToEndAsync();

        await process.WaitForExitAsync();

        var standardOutput = await stdOutTask;
        var standardError = await stdErrTask;

        if (process.ExitCode == 0)
        {
            return;
        }

        var output = string.Join(
            Environment.NewLine,
            new[] { standardOutput, standardError }
                .Where(text => !string.IsNullOrWhiteSpace(text))
                .Select(text => text.Trim()));

        throw new InvalidOperationException(
            string.IsNullOrWhiteSpace(output)
                ? failureMessage
                : $"{failureMessage}{Environment.NewLine}{output}");
    }

    private static BundleStatus GetPostgresBundleStatus(RepositoryLayout repositoryLayout)
    {
        var newestInput = GetNewestMigrationInput(repositoryLayout);
        var existingBundle = repositoryLayout.PostgresMigrationsBundleCandidatePaths
            .Where(File.Exists)
            .Select(path => new FileInfo(path))
            .OrderByDescending(file => file.LastWriteTimeUtc)
            .FirstOrDefault();

        if (existingBundle is null)
        {
            return new BundleStatus(
                BundlePath: null,
                BundleTimestampUtc: null,
                NewestInputPath: newestInput.Path,
                NewestInputTimestampUtc: newestInput.LastWriteTimeUtc,
                CanUseBundle: false);
        }

        var canUseBundle = existingBundle.LastWriteTimeUtc >= newestInput.LastWriteTimeUtc;
        return new BundleStatus(
            BundlePath: existingBundle.FullName,
            BundleTimestampUtc: existingBundle.LastWriteTimeUtc,
            NewestInputPath: newestInput.Path,
            NewestInputTimestampUtc: newestInput.LastWriteTimeUtc,
            CanUseBundle: canUseBundle);
    }

    private static MigrationInputFile GetNewestMigrationInput(RepositoryLayout repositoryLayout)
    {
        var candidateFiles = new[]
            {
                repositoryLayout.KitosContextPath,
                repositoryLayout.DesignTimeFactoryPath
            }
            .Concat(Directory.GetFiles(repositoryLayout.EfCoreMigrationsDirectoryPath, "*.cs"))
            .Select(path => new FileInfo(path))
            .Where(file => file.Exists)
            .ToList();

        var newest = candidateFiles
            .OrderByDescending(file => file.LastWriteTimeUtc)
            .FirstOrDefault();

        if (newest is null)
        {
            throw new InvalidOperationException("Could not determine the latest migration input file for PostgreSQL bundle freshness checks.");
        }

        return new MigrationInputFile(newest.FullName, newest.LastWriteTimeUtc);
    }

    private static string BuildDotnetEfFailureMessage(BundleStatus bundleStatus)
    {
        if (bundleStatus.BundlePath is null)
        {
            return "dotnet ef failed while applying PostgreSQL migrations. No PostgreSQL bundle was available as a fallback.";
        }

        if (!bundleStatus.CanUseBundle)
        {
            return
                "dotnet ef failed while applying PostgreSQL migrations. " +
                $"The available PostgreSQL bundle is stale ({bundleStatus.BundlePath}); rebuild it with DeploymentScripts/PrepareDbMigrationsBundle.ps1 or continue using dotnet ef from the current repo.";
        }

        return "dotnet ef failed while applying PostgreSQL migrations.";
    }
}

internal sealed record RepositoryLayout(
    string RootPath,
    string InfrastructureProjectPath,
    string StartupProjectPath,
    IReadOnlyList<string> PostgresMigrationsBundleCandidatePaths,
    string EfCoreMigrationsDirectoryPath,
    string KitosContextPath,
    string DesignTimeFactoryPath);

internal sealed record BundleStatus(
    string? BundlePath,
    DateTime? BundleTimestampUtc,
    string NewestInputPath,
    DateTime NewestInputTimestampUtc,
    bool CanUseBundle);

internal sealed record MigrationInputFile(
    string Path,
    DateTime LastWriteTimeUtc);

internal static class RepositoryLayoutLocator
{
    public static RepositoryLayout Find()
    {
        foreach (var startPath in GetStartPaths())
        {
            var repositoryLayout = TryFindFrom(startPath);
            if (repositoryLayout is not null)
            {
                return repositoryLayout;
            }
        }

        throw new InvalidOperationException("Could not locate the repository root containing DeploymentScripts and the EF Core projects.");
    }

    private static IEnumerable<string> GetStartPaths()
    {
        yield return AppContext.BaseDirectory;
        yield return Directory.GetCurrentDirectory();
        yield return typeof(RepositoryLayoutLocator).Assembly.Location;
    }

    private static RepositoryLayout? TryFindFrom(string startPath)
    {
        var directoryPath = File.Exists(startPath)
            ? Path.GetDirectoryName(startPath)
            : startPath;

        if (string.IsNullOrWhiteSpace(directoryPath))
        {
            return null;
        }

        for (var current = new DirectoryInfo(directoryPath); current is not null; current = current.Parent)
        {
            var infrastructureProjectPath = Path.Combine(current.FullName, "Infrastructure.DataAccess", "Infrastructure.DataAccess.csproj");
            var startupProjectPath = Path.Combine(current.FullName, "Presentation.Web", "Presentation.Web.csproj");
            var efCoreMigrationsDirectoryPath = Path.Combine(current.FullName, "Infrastructure.DataAccess", "Migrations", "EfCore");
            var kitosContextPath = Path.Combine(current.FullName, "Infrastructure.DataAccess", "KitosContext.cs");
            var designTimeFactoryPath = Path.Combine(current.FullName, "Infrastructure.DataAccess", "KitosContextDesignTimeFactory.cs");
            if (!File.Exists(infrastructureProjectPath)
                || !File.Exists(startupProjectPath)
                || !Directory.Exists(efCoreMigrationsDirectoryPath)
                || !File.Exists(kitosContextPath)
                || !File.Exists(designTimeFactoryPath))
            {
                continue;
            }

            return new RepositoryLayout(
                current.FullName,
                infrastructureProjectPath,
                startupProjectPath,
                [
                    Path.Combine(current.FullName, "MigrationsBundle", "efbundle.postgresql.exe"),
                    Path.Combine(current.FullName, "Output", "MigrationsBundle", "efbundle.postgresql.exe")
                ],
                efCoreMigrationsDirectoryPath,
                kitosContextPath,
                designTimeFactoryPath);
        }

        return null;
    }
}
