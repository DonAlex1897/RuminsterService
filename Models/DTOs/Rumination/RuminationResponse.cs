using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RuminsterBackend.Models.DTOs.User;

namespace RuminsterBackend.Models.DTOs.Rumination
{
    public class RuminationResponse
    {
        public int Id { get; set; }

        public string Content { get; set; }

        public bool IsPublic { get; set; }

        public List<RuminationAudienceResponse> Audiences { get; set; }

        public UserResponse CreatedBy { get; set; }

        public UserResponse UpdatedBy { get; set; }

        public DateTime CreateTMS { get; set; }

        public DateTime UpdateTMS { get; set; }

    }
}