using Tools.MigrateSQLServer2Postgres.Cli;
using Tools.MigrateSQLServer2Postgres.Migration;
using Tools.MigrateSQLServer2Postgres.PubSub.Cli;
using Tools.MigrateSQLServer2Postgres.PubSub.Migration;

namespace Tools.MigrateSQLServer2Postgres;

internal static class Program
{
    private static async Task<int> Main(string[] args)
    {
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

        var isPubSub = CommandLineParser.IsPubSubSubcommand(args);
        var effectiveArgs = CommandLineParser.StripSubcommand(args);

        if (CommandLineParser.IsHelpRequested(effectiveArgs))
        {
            if (isPubSub)
                CliConsole.RenderPubSubHelp();
            else
                CliConsole.RenderHelp();
            return 0;
        }

        try
        {
            var isInteractive = CommandLineParser.IsInteractiveRequested(effectiveArgs);

            if (isPubSub)
            {
                var options = isInteractive
                    ? PubSubInteractivePrompt.PromptForOptions(effectiveArgs)
                    : CommandLineParser.Parse(effectiveArgs);
                var runner = new PubSubMigrationRunner();
                return await runner.RunAsync(options, isInteractive);
            }
            else
            {
                var options = isInteractive
                    ? InteractivePrompt.PromptForOptions(effectiveArgs)
                    : CommandLineParser.Parse(effectiveArgs);
                var runner = new MigrationRunner();
                return await runner.RunAsync(options, isInteractive);
            }
        }
        catch (CommandLineException cliException)
        {
            CliConsole.Error(cliException.Message);
            if (isPubSub)
                CliConsole.RenderPubSubHelp();
            else
                CliConsole.RenderHelp();
            return -1;
        }
        catch (Exception exception)
        {
            CliConsole.Exception("Migration failed with an unexpected error.", exception);
            return -2;
        }
    }
}
