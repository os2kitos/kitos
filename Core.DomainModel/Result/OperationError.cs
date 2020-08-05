using Infrastructure.Services.Types;

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

        protected bool Equals(OperationError other)
        {
            return Equals(Message, other.Message) && FailureType == other.FailureType;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((OperationError) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((Message != null ? Message.GetHashCode() : 0) * 397) ^ (int) FailureType;
            }
        }

        public override string ToString()
        {
            return $"{FailureType:G}{Message.GetValueOrFallback(string.Empty)}";
        }
    }
}
