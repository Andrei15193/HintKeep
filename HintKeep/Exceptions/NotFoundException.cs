using System;

namespace HintKeep.Exceptions
{
    public class NotFoundException : ApplicationException
    {
        public NotFoundException()
            : base(string.Empty)
        {
        }

        public NotFoundException(string message)
            : base(message)
        {
        }

        public NotFoundException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        public NotFoundException(Exception innerException)
            : base(string.Empty, innerException)
        {
        }
    }
}