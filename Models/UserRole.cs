using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace RuminsterBackend.Models
{
    public class UserRole : IdentityUserRole<string>
    {
        public override string UserId { get; set; }

        public User User { get; set; }

        public override string RoleId { get; set; }

        public Role Role { get; set; }
    }
}