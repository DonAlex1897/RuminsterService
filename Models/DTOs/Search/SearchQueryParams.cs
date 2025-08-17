using System.ComponentModel.DataAnnotations;

namespace RuminsterBackend.Models.DTOs.Search
{
    public class SearchQueryParams
    {
        [Required]
        public string Query { get; set; } = string.Empty;

        public int? UsersLimit { get; set; } = 10;

        public int? UsersOffset { get; set; }

        public int? RuminationsLimit { get; set; } = 10;

        public int? RuminationsOffset { get; set; }
    }
}
