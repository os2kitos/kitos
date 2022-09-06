namespace Core.Abstractions.Types
{
    public class DetailedOperationError<TDetail> : OperationError
    {
        public DetailedOperationError(OperationFailure failureType, TDetail detail, string message = null) 
            : base(message, failureType)
        {
            Detail = detail;
        }

        public TDetail Detail { get; }
    }
}
