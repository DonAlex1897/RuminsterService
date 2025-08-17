using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using System.ComponentModel.DataAnnotations;

namespace RuminsterBackend.Models.DTOs.Auth
{
    public class PostSignUpDto
    {
        [Required, MinLength(3)]
        public string Username { get; set; } = string.Empty;
        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;
        [Required, MinLength(8)]
        public string Password { get; set; } = string.Empty;
        [Required]
        public string AcceptedTosVersion { get; set; } = string.Empty;
    }
}