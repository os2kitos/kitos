using System.Text.Json;
using Spectre.Console;

namespace Tools.MigrateSQLServer2Postgres.Cli;

internal static class InteractivePrompt
{
    public static CommandLineOptions PromptForOptions(string[]? args = null)
    {
        CliConsole.RenderHeader();

        // Extract command-line flags for interactive mode
        var cliFlags = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        if (args != null)
        {
            foreach (var arg in args)
            {
                if (arg.Equals("--allow-non-empty-target", StringComparison.OrdinalIgnoreCase))
                    cliFlags.Add("Allow non-empty target");
                else if (arg.Equals("--continue-on-error", StringComparison.OrdinalIgnoreCase))
                    cliFlags.Add("Continue on table error");
            }
        }

        var savedDefaults = InteractiveSettingsStore.Load();

        // If CLI flags are provided, skip profile selection and use saved defaults (or create empty)
        if (cliFlags.Count > 0)
        {
            var presetDefaults = savedDefaults ?? InteractiveSettingsStore.CreateEmptyDefaults();
            var presetOptions = presetDefaults with
            {
                AllowNonEmptyTarget = cliFlags.Contains("Allow non-empty target") || presetDefaults.AllowNonEmptyTarget,
                ContinueOnError = cliFlags.Contains("Continue on table error") || presetDefaults.ContinueOnError
            };
            
            CliConsole.Info("Using saved connection settings with command-line flags.");
            AnsiConsole.Write(BuildSummaryTable(presetOptions));
            return presetOptions;
        }

        // No CLI flags: show profile selection
        var profileChoices = new List<string>();
        if (savedDefaults is not null)
        {
            profileChoices.Add("Use last interactive settings");
        }

        profileChoices.Add("Use local development preset");
        profileChoices.Add("Enter settings manually");

        var selectedProfile = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("Choose [green]interactive setup profile[/]")
                .PageSize(10)
                .AddChoices(profileChoices));

        var defaults = selectedProfile switch
        {
            "Use last interactive settings" when savedDefaults is not null => savedDefaults,
            "Use local development preset" => InteractiveSettingsStore.CreateLocalDevelopmentPreset(),
            _ => InteractiveSettingsStore.CreateEmptyDefaults()
        };

        // Short-circuit for presets: show summary and confirm without re-asking every question
        if (selectedProfile is "Use last interactive settings" or "Use local development preset")
        {
            AnsiConsole.Write(BuildSummaryTable(defaults));

            if (!AnsiConsole.Confirm("Proceed with these settings?", true))
            {
                throw new CommandLineException("Interactive setup cancelled.");
            }

            InteractiveSettingsStore.Save(defaults);
            return defaults;
        }

        var sourceConnectionString = AnsiConsole.Prompt(
            new TextPrompt<string>("Enter [green]SQL Server source[/] connection string")
                .PromptStyle("deepskyblue2")
                .ValidationErrorMessage("[red]A source connection string is required[/]")
                .DefaultValue(defaults.SourceConnectionString)
                .ShowDefaultValue()
                .Validate(value => string.IsNullOrWhiteSpace(value) ? ValidationResult.Error() : ValidationResult.Success()));

        var targetConnectionString = AnsiConsole.Prompt(
            new TextPrompt<string>("Enter [green]PostgreSQL target[/] connection string")
                .PromptStyle("deepskyblue2")
                .ValidationErrorMessage("[red]A target connection string is required[/]")
                .DefaultValue(defaults.TargetConnectionString)
                .ShowDefaultValue()
                .Validate(value => string.IsNullOrWhiteSpace(value) ? ValidationResult.Error() : ValidationResult.Success()));

            var selectedFlagLookup = PromptForFlags(defaults);
            
            // Merge CLI flags with user-selected flags (CLI flags take precedence)
            foreach (var cliFlag in cliFlags)
            {
                selectedFlagLookup.Add(cliFlag);
            }

        var options = new CommandLineOptions(
            sourceConnectionString,
            targetConnectionString,
            selectedFlagLookup.Contains("Allow non-empty target"),
            selectedFlagLookup.Contains("Continue on table error"));

        AnsiConsole.Write(BuildSummaryTable(options));

        if (!AnsiConsole.Confirm("Proceed with these settings?", true))
        {
            throw new CommandLineException("Interactive setup cancelled.");
        }

        InteractiveSettingsStore.Save(options);

        return options;
    }

    private static HashSet<string> PromptForFlags(CommandLineOptions defaults)
    {
        var availableFlags = new[]
        {
            "Allow non-empty target",
            "Continue on table error"
        };

        if (availableFlags.Length == 0)
        {
            return new HashSet<string>(StringComparer.Ordinal);
        }

        var flagPrompt = new MultiSelectionPrompt<string>()
            .Title("Toggle optional [green]migration flags[/]")
            .InstructionsText("[grey](Use arrow keys to move, space to toggle, enter to confirm)[/]")
            .NotRequired()
            .PageSize(10)
            .AddChoices(availableFlags);

        if (defaults.AllowNonEmptyTarget && availableFlags.Contains("Allow non-empty target", StringComparer.Ordinal))
        {
            flagPrompt.Select("Allow non-empty target");
        }

        if (defaults.ContinueOnError && availableFlags.Contains("Continue on table error", StringComparer.Ordinal))
        {
            flagPrompt.Select("Continue on table error");
        }

        var selectedFlags = AnsiConsole.Prompt(flagPrompt);
        return new HashSet<string>(selectedFlags, StringComparer.Ordinal);
    }

    private static Table BuildSummaryTable(CommandLineOptions options)
    {
        var summary = new Table().Border(TableBorder.Rounded).AddColumn("Setting").AddColumn("Value");
        summary.AddRow("Source", TruncateForDisplay(options.SourceConnectionString));
        summary.AddRow("Target", TruncateForDisplay(options.TargetConnectionString));
        summary.AddRow("Allow non-empty target", options.AllowNonEmptyTarget ? "Yes" : "No");
        summary.AddRow("Continue on error", options.ContinueOnError ? "Yes" : "No");
        return summary;
    }

    private static string TruncateForDisplay(string value)
    {
        const int maxLength = 96;
        if (string.IsNullOrWhiteSpace(value) || value.Length <= maxLength)
        {
            return value;
        }

        return value[..(maxLength - 3)] + "...";
    }
}

internal static class InteractiveSettingsStore
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = true
    };

    private static string SettingsPath => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "Tools.MigrateSQLServer2Postgres",
        "interactive-defaults.json");

    public static CommandLineOptions? Load()
    {
        try
        {
            if (!File.Exists(SettingsPath))
            {
                return null;
            }

            var json = File.ReadAllText(SettingsPath);
            return JsonSerializer.Deserialize<CommandLineOptions>(json, SerializerOptions);
        }
        catch
        {
            return null;
        }
    }

    public static void Save(CommandLineOptions options)
    {
        var directory = Path.GetDirectoryName(SettingsPath);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var json = JsonSerializer.Serialize(options, SerializerOptions);
        File.WriteAllText(SettingsPath, json);
    }

    public static CommandLineOptions CreateLocalDevelopmentPreset()
    {
        return new CommandLineOptions(
            "Server=.\\SQLEXPRESS;Database=Kitos;Integrated Security=true;TrustServerCertificate=true",
            "Host=127.0.0.1;Port=5432;Database=kitos;Username=postgres;Password=postgres",
            false,
            false);
    }

    public static CommandLineOptions CreateEmptyDefaults()
    {
        return new CommandLineOptions(
            string.Empty,
            string.Empty,
            false,
            false);
    }
}
