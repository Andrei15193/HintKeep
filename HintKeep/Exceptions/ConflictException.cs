using System;

namespace HintKeep.Exceptions
{
    public class ConflictException : ApplicationException
    {
        public ConflictException()
            : base (string.Empty)
        {
        }

        public ConflictException(string message)
            : base(message)
        {
        }

        public ConflictException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        public ConflictException(Exception innerException)
            : base(string.Empty, innerException)
        {
        }
    }
}