using System.Collections.Generic;
using RuminsterBackend.Models.DTOs.Rumination;
using RuminsterBackend.Models.DTOs.User;

namespace RuminsterBackend.Models.DTOs.Search
{
    public class SearchResponse
    {
        public List<UserResponse> Users { get; set; } = new();
        public List<RuminationResponse> Ruminations { get; set; } = new();
    }
}
