namespace Tools.MigrateSQLServer2Postgres.Cli;

internal static class CommandLineParser
{
    private const string PubSubSubcommand = "pubsub";

    public static IReadOnlyList<(string Argument, string Description)> HelpItems { get; } =
    [
        ("--source <sqlServerConnectionString>", "SQL Server source connection string."),
        ("--target <postgresConnectionString>", "PostgreSQL target connection string."),
        ("--allow-non-empty-target", "Allow migration into a non-empty PostgreSQL target database."),
        ("--continue-on-error", "Continue to next table when a table migration fails."),
        ("--interactive", "Launch interactive wizard with arrow-key navigation."),
        ("--help", "Show help.")
    ];

    public static bool IsPubSubSubcommand(string[] args)
    {
        return args.Length > 0 && args[0].Equals(PubSubSubcommand, StringComparison.OrdinalIgnoreCase);
    }

    public static string[] StripSubcommand(string[] args)
    {
        return args.Length > 0 && IsPubSubSubcommand(args) ? args[1..] : args;
    }

    public static bool IsHelpRequested(string[] args)
    {
        return args.Any(arg => arg.Equals("--help", StringComparison.OrdinalIgnoreCase) || arg.Equals("-h", StringComparison.OrdinalIgnoreCase));
    }

    public static bool IsInteractiveRequested(string[] args)
    {
        return args.Length == 0 || args.Any(arg => arg.Equals("--interactive", StringComparison.OrdinalIgnoreCase) || arg.Equals("-i", StringComparison.OrdinalIgnoreCase));
    }

    public static CommandLineOptions Parse(string[] args)
    {
        if (args.Length == 0)
        {
            throw new CommandLineException("Missing required arguments.");
        }

        var values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var flags = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        for (var i = 0; i < args.Length; i++)
        {
            var arg = args[i];
            if (!arg.StartsWith("--", StringComparison.Ordinal))
            {
                throw new CommandLineException($"Unexpected argument '{arg}'. Use --help to see supported arguments.");
            }

            if (arg.Equals("--interactive", StringComparison.OrdinalIgnoreCase) || arg.Equals("-i", StringComparison.OrdinalIgnoreCase))
            {
                flags.Add(arg);
                continue;
            }

            if (i + 1 < args.Length && !args[i + 1].StartsWith("--", StringComparison.Ordinal))
            {
                values[arg] = args[i + 1];
                i++;
            }
            else
            {
                flags.Add(arg);
            }
        }

        var sourceConnectionString = GetRequired(values, "--source");
        var targetConnectionString = GetRequired(values, "--target");

        var allowNonEmptyTarget = flags.Contains("--allow-non-empty-target");
        var continueOnError = flags.Contains("--continue-on-error");

        return new CommandLineOptions(
            sourceConnectionString,
            targetConnectionString,
            allowNonEmptyTarget,
            continueOnError);
    }

    private static string GetRequired(IReadOnlyDictionary<string, string> values, string key)
    {
        if (values.TryGetValue(key, out var value) && string.IsNullOrWhiteSpace(value) == false)
        {
            return value;
        }

        throw new CommandLineException($"Missing required argument '{key}'.");
    }
}

internal sealed class CommandLineException(string message) : Exception(message);
