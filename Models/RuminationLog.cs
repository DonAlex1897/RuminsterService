using Microsoft.AspNetCore.Identity;

namespace RuminsterBackend.Models
{
    public class RuminationLog
    {
        public int Id { get; set; }

        public int RuminationId { get; set; }

        public bool IsDeleted { get; set; }

        public Rumination Rumination { get; set; }

        public string Content { get; set; }

        public bool IsPublished { get; set; }

        public string CallerMethod { get; set; }

        public string CreateById { get; set; }

        public User CreateBy { get; set; }

        public DateTime CreateTMS { get; set; }
    }
}