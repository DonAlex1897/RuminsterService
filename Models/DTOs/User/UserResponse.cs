using RuminsterBackend.Models.DTOs.Role;

namespace RuminsterBackend.Models.DTOs.User
{
    public class UserResponse
    {
        public string Id { get; set; }

        public string Username { get; set; }

        public string Email { get; set; }

    public string? Name { get; set; }

        public List<string>? Roles { get; set; }
    }
}