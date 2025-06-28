using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace RuminsterBackend.Models
{
    public class TermsOfService
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        [MaxLength(50)]
        public string Version { get; set; }
        
        [Required]
        public string Content { get; set; }
        
        [Required]
        public DateTime CreatedAt { get; set; }
        
        [Required]
        public bool IsActive { get; set; }
        
        public virtual ICollection<UserTosAcceptance> UserAcceptances { get; set; }
    }
}
