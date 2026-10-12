namespace Core.Abstractions.Types
{
    public class DetailedOperationError<TDetail>(OperationFailure failureType, TDetail detail, string? message = null)
        : OperationError(message, failureType)
    {
        public TDetail Detail { get; } = detail;
    }
}
