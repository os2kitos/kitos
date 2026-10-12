using Spectre.Console;
using Tools.MigrateSQLServer2Postgres.Migration;

namespace Tools.MigrateSQLServer2Postgres.Cli;

internal static class CliConsole
{
    public static void RenderHeader()
    {
        AnsiConsole.Write(new FigletText("KITOS Migrator")
            .LeftJustified()
            .Color(Color.SteelBlue1));

        AnsiConsole.Write(new Panel(new Markup("[grey]SQL Server -> PostgreSQL copy-over migration tool[/]"))
            .Border(BoxBorder.Rounded)
            .BorderColor(Color.Grey));
    }

    public static void RenderHelp()
    {
        RenderHeader();

        var table = new Table().Border(TableBorder.Rounded).AddColumn("Argument").AddColumn("Description");
        foreach (var item in CommandLineParser.HelpItems)
        {
            table.AddRow($"[deepskyblue2]{Markup.Escape(item.Argument)}[/]", Markup.Escape(item.Description));
        }

        AnsiConsole.Write(new Markup("[bold]Usage[/]\n"));
        AnsiConsole.WriteLine("  Tools.MigrateSQLServer2Postgres --source <sqlServerConnectionString> --target <postgresConnectionString> [options]");
        AnsiConsole.WriteLine();
        AnsiConsole.Write(table);
    }

    public static void RenderPubSubHeader()
    {
        AnsiConsole.Write(new FigletText("PubSub Migrator")
            .LeftJustified()
            .Color(Color.SteelBlue1));

        AnsiConsole.Write(new Panel(new Markup("[grey]SQL Server -> PostgreSQL copy-over migration tool for PubSub[/]"))
            .Border(BoxBorder.Rounded)
            .BorderColor(Color.Grey));
    }

    public static void RenderPubSubHelp()
    {
        RenderPubSubHeader();

        var table = new Table().Border(TableBorder.Rounded).AddColumn("Argument").AddColumn("Description");
        foreach (var item in CommandLineParser.HelpItems)
        {
            table.AddRow($"[deepskyblue2]{Markup.Escape(item.Argument)}[/]", Markup.Escape(item.Description));
        }

        AnsiConsole.Write(new Markup("[bold]Usage[/]\n"));
        AnsiConsole.WriteLine("  Tools.MigrateSQLServer2Postgres pubsub --source <sqlServerConnectionString> --target <postgresConnectionString> [options]");
        AnsiConsole.WriteLine();
        AnsiConsole.Write(table);
    }

    public static void Info(string message)
    {
        AnsiConsole.MarkupLine($"[deepskyblue2]{Markup.Escape(message)}[/]");
    }

    public static void Success(string message)
    {
        AnsiConsole.MarkupLine($"[green]{Markup.Escape(message)}[/]");
    }

    public static void Warning(string message)
    {
        AnsiConsole.MarkupLine($"[yellow]{Markup.Escape(message)}[/]");
    }

    public static void Error(string message)
    {
        AnsiConsole.MarkupLine($"[red]{Markup.Escape(message)}[/]");
    }

    public static void Exception(string title, Exception exception)
    {
        Error(title);
        AnsiConsole.WriteException(exception, ExceptionFormats.ShortenEverything | ExceptionFormats.ShowLinks);
    }

    public static void RenderReadinessSummary(int sourceTableCount, int targetTableCount, int commonTableCount, bool targetIsEmpty, TableRef? firstNonEmptyTable)
    {
        var summary = new Table()
            .Border(TableBorder.Rounded)
            .AddColumn("Check")
            .AddColumn("Result");

        summary.AddRow("Source candidate tables", sourceTableCount.ToString());
        summary.AddRow("Target candidate tables", targetTableCount.ToString());
        summary.AddRow("Resolved matched tables", commonTableCount.ToString());
        summary.AddRow("Target empty", targetIsEmpty ? "[green]Yes[/]" : "[yellow]No[/]");

        if (firstNonEmptyTable is not null)
        {
            summary.AddRow("First populated target table", $"[yellow]{Markup.Escape(firstNonEmptyTable.ToString())}[/]");
        }

        AnsiConsole.Write(summary);
    }

    public static void RenderValidationSummary(int comparedTableCount, IReadOnlyCollection<ValidationMismatch> mismatches)
    {
        var summary = new Table()
            .Border(TableBorder.Rounded)
            .AddColumn("Metric")
            .AddColumn("Value");

        summary.AddRow("Compared tables", comparedTableCount.ToString());
        summary.AddRow("Mismatches", mismatches.Count == 0 ? "[green]0[/]" : $"[red]{mismatches.Count}[/]");
        AnsiConsole.Write(summary);

        if (mismatches.Count == 0)
        {
            return;
        }

        var mismatchTable = new Table()
            .Border(TableBorder.Rounded)
            .AddColumn("Table")
            .AddColumn("Source rows")
            .AddColumn("Target rows");

        foreach (var mismatch in mismatches.Take(20))
        {
            mismatchTable.AddRow(
                Markup.Escape(mismatch.Table.ToString()),
                mismatch.SourceRowCount.ToString(),
                mismatch.TargetRowCount.ToString());
        }

        AnsiConsole.Write(mismatchTable);
    }

    public static void RenderExecutionSummary(IReadOnlyCollection<TableExecutionResult> results)
    {
        var summary = new Table()
            .Border(TableBorder.Rounded)
            .AddColumn("Metric")
            .AddColumn("Value");

        summary.AddRow("Processed tables", results.Count.ToString());
        summary.AddRow("Succeeded", $"[green]{results.Count(result => result.Status == TableExecutionStatus.Succeeded)}[/]");
        summary.AddRow("Skipped", $"[yellow]{results.Count(result => result.Status == TableExecutionStatus.Skipped)}[/]");
        summary.AddRow("Failed", $"[red]{results.Count(result => result.Status == TableExecutionStatus.Failed)}[/]");

        AnsiConsole.Write(summary);

        if (results.Count == 0)
        {
            return;
        }

        var resultTable = new Table()
            .Border(TableBorder.Rounded)
            .AddColumn("Table")
            .AddColumn("Status")
            .AddColumn("Source rows")
            .AddColumn("Copied rows")
            .AddColumn("Detail");

        foreach (var result in results)
        {
            var statusMarkup = result.Status switch
            {
                TableExecutionStatus.Succeeded => "[green]Succeeded[/]",
                TableExecutionStatus.Skipped => "[yellow]Skipped[/]",
                TableExecutionStatus.Failed => "[red]Failed[/]",
                _ => Markup.Escape(result.Status.ToString())
            };

            var mismatch = result.SourceRowCount.HasValue
                && result.RowsCopied.HasValue
                && result.SourceRowCount.Value != result.RowsCopied.Value;

            var sourceRowsMarkup = result.SourceRowCount?.ToString() ?? "-";
            var copiedRowsMarkup = mismatch
                ? $"[red]{result.RowsCopied}[/]"
                : (result.RowsCopied?.ToString() ?? "-");

            resultTable.AddRow(
                Markup.Escape(result.Table.ToString()),
                statusMarkup,
                sourceRowsMarkup,
                copiedRowsMarkup,
                Markup.Escape(result.Detail ?? string.Empty));
        }

        AnsiConsole.Write(resultTable);
    }

    public static void RenderHints(string title, IEnumerable<string> hints)
    {
        var rows = hints.ToList();
        if (rows.Count == 0)
        {
            return;
        }

        var markup = string.Join("\n", rows.Select(hint => $"[yellow]-[/] {Markup.Escape(hint)}"));
        AnsiConsole.Write(new Panel(new Markup(markup))
            .Header($"[bold]{Markup.Escape(title)}[/]")
            .Border(BoxBorder.Rounded)
            .BorderColor(Color.Yellow));
    }
}
