using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RuminsterBackend.Models.DTOs.Auth
{
    public class PostRefreshTokenDto
    {
        public string RefreshToken { get; set; }
    }
}