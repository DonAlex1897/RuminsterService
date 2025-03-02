
namespace RuminsterBackend.Models.DTOs.User
{
    using Microsoft.AspNetCore.Identity;

    public static class UserMapper
    {
        public static UserResponse MapUserResponse(IdentityUser user, List<string>? roles = null)
        {
            return new UserResponse
            {
                Id = user.Id,
                Email = user.Email ?? string.Empty,
                Username = user.UserName ?? string.Empty,
                Roles = roles,
            };
        }
    }
}