using System;

namespace HintKeep.Exceptions
{
    public class UnauthorizedException : ApplicationException
    {
        public UnauthorizedException()
            : base (string.Empty)
        {
        }

        public UnauthorizedException(string message)
            : base(message)
        {
        }

        public UnauthorizedException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        public UnauthorizedException(Exception innerException)
            : base(string.Empty, innerException)
        {
        }
    }
}