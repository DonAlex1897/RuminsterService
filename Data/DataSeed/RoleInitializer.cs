using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using RuminsterBackend.Models;

namespace RuminsterBackend.Data.DataSeed
{
    public static class RoleInitializer
    {
        public static async Task InitializeRoles(RoleManager<Role> roleManager)
        {
            var roles = new[] { "Admin", "User", "Moderator" };

            foreach (var role in roles)
            {
                var roleExist = await roleManager.RoleExistsAsync(role);
                if (!roleExist)
                {
                    await roleManager.CreateAsync(new Role() { Name = role });
                }
            }
        }
    }
}