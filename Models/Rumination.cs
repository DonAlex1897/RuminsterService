using Microsoft.AspNetCore.Identity;

namespace RuminsterBackend.Models
{
    public class Rumination
    {
        public int Id { get; set; }

        public bool IsDeleted { get; set; }

        public string Content { get; set; }

        public bool IsPublic { get; set; }

        public string CreateById { get; set; }

        public IdentityUser CreateBy { get; set; }

        public string UpdateById { get; set; }

        public IdentityUser UpdateBy { get; set; }

        public DateTime CreateTMS { get; set; }

        public DateTime UpdateTMS { get; set; }

        public ICollection<RuminationAudience> Audiences { get; set; }

        public ICollection<RuminationLog> Logs { get; set; }
    }
}