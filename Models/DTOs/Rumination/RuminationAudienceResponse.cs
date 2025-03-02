using RuminsterBackend.Models.Enums;

namespace RuminsterBackend.Models.DTOs.Rumination
{
    public class RuminationAudienceResponse
    {
        public int Id { get; set; }

        public UserRelationType RelationType { get; set; }

        public DateTime CreateTMS { get; set; }

        public DateTime UpdateTMS { get; set; }

    }
}