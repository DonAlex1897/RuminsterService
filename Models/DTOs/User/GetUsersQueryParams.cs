using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RuminsterBackend.Models.Enums;

namespace RuminsterBackend.Models.DTOs.User
{
    public class GetUsersQueryParams
    {
        public List<string> Username { get; set; }

        public List<UserRelationType> Relation { get; set; }
    }
}