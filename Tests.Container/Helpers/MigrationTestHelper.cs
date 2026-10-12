using System.Diagnostics;
using Infrastructure.DataAccess;
using Microsoft.EntityFrameworkCore;

namespace Tests.Container.Helpers;

internal static class MigrationTestHelper
{
    private const string ConnectionStringEnvVar = "ConnectionStrings__KitosContext";
    private const string ProviderEnvVar = "Database__Provider";
    private const string IgnorePendingModelChangesWarningEnvVar = "IgnorePendingModelChangesWarning";

    public static async Task AssertNoPendingMigrationsAsync(
        string provider,
        string kitosConnectionString,
        string hangfireConnectionString)
    {
        var previousConnectionString = Environment.GetEnvironmentVariable(ConnectionStringEnvVar);
        var previousProvider = Environment.GetEnvironmentVariable(ProviderEnvVar);
        var previousIgnorePendingModelChangesWarning = Environment.GetEnvironmentVariable(IgnorePendingModelChangesWarningEnvVar);

        try
        {
            await ExecutePrepareLocalDatabaseScriptAsync(kitosConnectionString, hangfireConnectionString);

            Environment.SetEnvironmentVariable(ConnectionStringEnvVar, kitosConnectionString);
            Environment.SetEnvironmentVariable(ProviderEnvVar, provider);
            Environment.SetEnvironmentVariable(IgnorePendingModelChangesWarningEnvVar, "true");

            var factory = new KitosContextDesignTimeFactory();
            await using var context = factory.CreateDbContext([]);

            var pendingMigrations = await context.Database.GetPendingMigrationsAsync();
            Assert.Empty(pendingMigrations);
        }
        finally
        {
            Environment.SetEnvironmentVariable(ConnectionStringEnvVar, previousConnectionString);
            Environment.SetEnvironmentVariable(ProviderEnvVar, previousProvider);
            Environment.SetEnvironmentVariable(IgnorePendingModelChangesWarningEnvVar, previousIgnorePendingModelChangesWarning);
        }
    }

    private static async Task ExecutePrepareLocalDatabaseScriptAsync(string kitosConnectionString, string hangfireConnectionString)
    {
        var repositoryRoot = FindRepositoryRoot();
        var scriptPath = Path.Combine(repositoryRoot, "DeploymentScripts", "PrepareLocalDatabase.ps1");
        var startInfo = new ProcessStartInfo
        {
            FileName = OperatingSystem.IsWindows() ? "powershell" : "pwsh",
            WorkingDirectory = repositoryRoot,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };

        startInfo.ArgumentList.Add("-NoProfile");
        startInfo.ArgumentList.Add("-ExecutionPolicy");
        startInfo.ArgumentList.Add("Bypass");
        startInfo.ArgumentList.Add("-File");
        startInfo.ArgumentList.Add(scriptPath);
        startInfo.ArgumentList.Add("-kitosDbConnectionString");
        startInfo.ArgumentList.Add(kitosConnectionString);
        startInfo.ArgumentList.Add("-hangfireDbConnectionString");
        startInfo.ArgumentList.Add(hangfireConnectionString);

        using var process = Process.Start(startInfo);
        Assert.NotNull(process);

        var stdOutTask = process.StandardOutput.ReadToEndAsync();
        var stdErrTask = process.StandardError.ReadToEndAsync();

        await process.WaitForExitAsync();

        var stdOut = await stdOutTask;
        var stdErr = await stdErrTask;
        var output = string.Join(
            Environment.NewLine,
            new[] { stdOut, stdErr }.Where(text => !string.IsNullOrWhiteSpace(text)).Select(text => text.Trim()));

        Assert.True(
            process.ExitCode == 0,
            $"PrepareLocalDatabase.ps1 failed with exit code {process.ExitCode}.{Environment.NewLine}{output}");
    }

    private static string FindRepositoryRoot()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);

        while (current is not null)
        {
            var candidate = Path.Combine(current.FullName, "DeploymentScripts", "PrepareLocalDatabase.ps1");
            if (File.Exists(candidate))
            {
                return current.FullName;
            }

            current = current.Parent;
        }

        throw new DirectoryNotFoundException("Could not locate repository root containing DeploymentScripts\\PrepareLocalDatabase.ps1");
    }
}
