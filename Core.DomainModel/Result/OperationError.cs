namespace Core.DomainModel.Result
{
    public class OperationError
    {
        public Maybe<string> Message { get; }
        public OperationFailure FailureType { get; }

        public static implicit operator OperationError(OperationFailure failure)
        {
            return new OperationError(Maybe<string>.None, failure);
        }

        private OperationError(Maybe<string> message, OperationFailure failureType)
        {
            Message = message;
            FailureType = failureType;
        }

        public OperationError(string message, OperationFailure failureType)
            : this(message.FromNullable(), failureType)
        {

        }

        public OperationError(OperationFailure failureType)
            : this(Maybe<string>.None, failureType)
        {

        }

        public override string ToString()
        {
            return $"{FailureType:G}{Message.GetValueOrFallback(string.Empty)}";
        }
    }
}
