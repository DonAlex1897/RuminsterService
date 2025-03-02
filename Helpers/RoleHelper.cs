using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RuminsterBackend.Models.Enums;

namespace RuminsterBackend.Helpers
{
    public static class RoleHelper
    {
        public static string GetRoleName(Role role)
        {
            return role.ToString();
        }

        public static Role GetRoleFromString(string role)
        {
            return Enum.TryParse<Role>(role, out var parsedRole) ? parsedRole : Role.User;
        }
    }
}