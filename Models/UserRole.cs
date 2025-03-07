using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace RuminsterBackend.Models
{
    public class UserRole : IdentityUserRole<string>
    {
        public virtual User User { get; set; }
        
        public virtual Role Role { get; set; }
    }
}