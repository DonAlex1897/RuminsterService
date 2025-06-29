using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RuminsterBackend.Exceptions
{
    public class UnauthorizedException : Exception
    {
        public UnauthorizedException(string message) : base(message)
        {
        }

        public UnauthorizedException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
