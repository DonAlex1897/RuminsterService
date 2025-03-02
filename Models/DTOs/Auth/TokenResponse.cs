using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RuminsterBackend.Models.DTOs.Auth
{
    public class TokenResponse
    {
        public required string AccessToken { get; set; }

        public required string RefreshToken { get; set; }
    }
}