using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RuminsterBackend.Models.DTOs.Auth
{
    public class PostSignUpDto
    {
        public string Username { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string AcceptedTosVersion { get; set; }
    }
}