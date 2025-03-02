using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace RuminsterBackend.Data.DataSeed
{
    public static class RoleInitializer
    {
        public static async Task InitializeRoles(RoleManager<IdentityRole> roleManager)
        {
            var roles = new[] { "Admin", "User", "Moderator" };

            foreach (var role in roles)
            {
                var roleExist = await roleManager.RoleExistsAsync(role);
                if (!roleExist)
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }
        }
    }
}