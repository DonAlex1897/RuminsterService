using System.ComponentModel.DataAnnotations;

namespace RuminsterBackend.Models.DTOs.Auth
{
    public class GetActivateDto
    {
        [Required]
        public string Token { get; set; } = string.Empty;
    }
}
