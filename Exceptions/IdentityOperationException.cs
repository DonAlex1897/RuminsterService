using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace RuminsterBackend.Exceptions
{
    public class IdentityOperationException  : Exception
    {
        public IEnumerable<IdentityError> Errors { get; }

        public IdentityOperationException(string message) : base(message)
        {
        }

        public IdentityOperationException(string message, IEnumerable<IdentityError> errors) 
            : base(message)
        {
            Errors = errors;
        }

        public IdentityOperationException(string message, Exception innerException) 
            : base(message, innerException)
        {
        }
    }
}