using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RuminsterBackend.Models.DTOs.User
{
    public class PostUserRolesDto
    {
        public string UserId { get; set; }

        public List<string> Roles { get; set; }
    }
}