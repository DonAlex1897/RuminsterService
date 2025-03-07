
namespace RuminsterBackend.Models.DTOs.User
{
    using RuminsterBackend.Models;
    public static class UserMapper
    {
        public static UserResponse MapUserResponse(User user)
        {
            return new UserResponse
            {
                Id = user.Id,
                Email = user.Email ?? string.Empty,
                Username = user.UserName ?? string.Empty,
                Roles = user.UserRoles?.Select(ur => ur.Role?.Name ?? string.Empty).ToList() ?? [],
            };
        }
    }
}