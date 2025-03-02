using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RuminsterBackend.Models.DTOs.User;

namespace RuminsterBackend.Models.DTOs.Rumination
{
    public class GetMyRuminationsQueryParams
    {

        public List<string>? Content { get; set; }

        public bool IsPublic { get; set; }

        public DateTime? FromTMS { get; set; }

        public DateTime? ToTMS { get; set; }

        public bool? IncludeDeleted { get; set; }

        public string? Sort { get; set; }

        public int? Limit { get; set; }

        public int? Offset { get; set; }

    }
}