using System;

namespace Presentation.Web.Models.Exceptions
{
    public class ConflictException : Exception
    {
        public ConflictException()
        {
            
        }

        public ConflictException(string message)
            : base(message)
        {
            
        }
    }
}