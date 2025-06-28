using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RuminsterBackend.Models
{
    public class UserTosAcceptance
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public string UserId { get; set; }
        
        [ForeignKey("UserId")]
        public virtual User User { get; set; }
        
        [Required]
        public int TermsOfServiceId { get; set; }
        
        [ForeignKey("TermsOfServiceId")]
        public virtual TermsOfService TermsOfService { get; set; }
        
        [Required]
        public DateTime AcceptedAt { get; set; }
        
        [Required]
        [MaxLength(50)]
        public string AcceptedVersion { get; set; }
    }
}
