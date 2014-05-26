using System;

namespace UI.MVC4.Models.Exceptions
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