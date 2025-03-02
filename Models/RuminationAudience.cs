using Microsoft.AspNetCore.Identity;
using RuminsterBackend.Models.Enums;

namespace RuminsterBackend.Models
{
    public class RuminationAudience
    {
        public int Id { get; set; }

        public bool IsDeleted { get; set; }

        public int RuminationId { get; set; }

        public Rumination Rumination { get; set; }

        public UserRelationType RelationType { get; set; }

        public DateTime CreateTMS { get; set; }

        public DateTime UpdateTMS { get; set; }
    }
}