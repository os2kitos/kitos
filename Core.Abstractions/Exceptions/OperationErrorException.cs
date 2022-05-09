using System;
using System.Runtime.Serialization;

namespace Core.Abstractions.Exceptions
{
    [Serializable]
    public class OperationErrorException<TError> : ApplicationException
    {
        public TError Error { get; set; }

        public OperationErrorException(TError error)
        {
            Error = error;
        }

        public OperationErrorException(string message, TError error) : base(message)
        {
            Error = error;
        }

        public OperationErrorException(string message, Exception innerException, TError error) : base(message, innerException)
        {
            Error = error;
        }

        protected OperationErrorException(SerializationInfo info, StreamingContext context, TError error) : base(info, context)
        {
            Error = error;
        }
    }
}
