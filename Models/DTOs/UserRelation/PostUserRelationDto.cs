using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RuminsterBackend.Models.Enums;

namespace RuminsterBackend.Models.DTOs.UserRelation
{
    public class PostUserRelationDto
    {
        public string UserId { get; set; }

        public UserRelationType RelationType { get; set; }
    }
}