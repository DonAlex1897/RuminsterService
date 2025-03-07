using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RuminsterBackend.Models.Enums;

namespace RuminsterBackend.Helpers
{
    public static class RoleHelper
    {
        public static string GetRoleName(RoleType role)
        {
            return role.ToString();
        }

        public static RoleType GetRoleFromString(string role)
        {
            return Enum.TryParse<RoleType>(role, out var parsedRole) ? parsedRole : RoleType.User;
        }
    }
}