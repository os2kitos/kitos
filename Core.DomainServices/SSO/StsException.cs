using System;
using System.Runtime.Serialization;

namespace Core.ApplicationServices.SSO
{
    public class STSException: SystemException
    {
        public STSException()
        {
        }

        public STSException(string message) : base(message)
        {
        }

        public STSException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected STSException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}