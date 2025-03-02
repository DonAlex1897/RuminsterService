using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace RuminsterBackend.Models
{
    public class RefreshToken
    {
        public int Id { get; set; }

        public string UserId { get; set; }

        public IdentityUser User { get; set; }

        public string Token  { get; set; }

        public DateTime ExpiresAt  { get; set; }

        public bool IsRevoked  { get; set; }

        public DateTime CreateTMS { get; set; }
    }
}