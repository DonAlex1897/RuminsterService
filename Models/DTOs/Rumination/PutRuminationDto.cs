using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RuminsterBackend.Models.Enums;

namespace RuminsterBackend.Models.DTOs.Rumination
{
    public class PutRuminationDto
    {
        public string Content { get; set; }

        public bool Publish { get; set; }

        public List<UserRelationType>? Audiences { get; set; }
    }
}
