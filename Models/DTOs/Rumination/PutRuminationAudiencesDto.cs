using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RuminsterBackend.Models.Enums;

namespace RuminsterBackend.Models.DTOs.Rumination
{
    public class PutRuminationAudiencesDto
    {
        public List<UserRelationType>? Audiences { get; set; }
    }
}