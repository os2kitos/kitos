namespace Tools.MigrateSQLServer2Postgres.Migration;

internal enum TableExecutionStatus
{
    Succeeded,
    Skipped,
    Failed
}

internal sealed record TableExecutionResult(TableRef Table, TableExecutionStatus Status, long? SourceRowCount = null, long? RowsCopied = null, string? Detail = null);

internal sealed record ValidationMismatch(TableRef Table, long SourceRowCount, long TargetRowCount);
