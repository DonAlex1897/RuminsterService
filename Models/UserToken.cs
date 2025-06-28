using System.ComponentModel.DataAnnotations;
using RuminsterBackend.Models.Enums;

namespace RuminsterBackend.Models
{
    public class UserToken
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public string UserId { get; set; }
        
        [Required]
        public string Token { get; set; }
        
        [Required]
        public UserTokenType TokenType { get; set; }
        
        [Required]
        public DateTime ExpiresAt { get; set; }
        
        [Required]
        public DateTime CreatedAt { get; set; }
        
        public bool IsUsed { get; set; }
        
        // Navigation property
        public virtual User User { get; set; }
    }
}
