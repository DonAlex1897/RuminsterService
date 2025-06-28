using System.ComponentModel.DataAnnotations;

namespace RuminsterBackend.Models.DTOs.Auth
{
    public class PostForgotPasswordDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
    }
}
