using System;

namespace HintKeep.Exceptions
{
    public class PreconditionFailedException : ApplicationException
    {
        public PreconditionFailedException()
            : base(string.Empty)
        {
        }

        public PreconditionFailedException(string message)
            : base(message)
        {
        }

        public PreconditionFailedException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        public PreconditionFailedException(Exception innerException)
            : base(string.Empty, innerException)
        {
        }
    }
}