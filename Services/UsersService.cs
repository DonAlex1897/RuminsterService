using Microsoft.AspNetCore.Identity;
using RuminsterBackend.Exceptions;
using RuminsterBackend.Models.DTOs.User;
using RuminsterBackend.Services.Interfaces;

namespace RuminsterBackend.Services
{
    public class UsersService(
        UserManager<IdentityUser> userManager,
        IHttpContextAccessor httpContextAccessor
    ) : IUsersService
    {
        private readonly UserManager<IdentityUser> _userManager = userManager;
        private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

        public async Task<UserResponse> GetCurrentUserAsync()
        {
            // Ensure the user is authenticated
            var httpContext = _httpContextAccessor.HttpContext
                ?? throw new UnauthorizedAccessException("No active user session.");

            var user = httpContext.User;
            if (user == null || !(user?.Identity?.IsAuthenticated ?? false))
            {
                throw new AuthenticationException("User not authenticated");
            }

            var identityUser = await _userManager.GetUserAsync(user) ??
                throw new NotFoundException("User not found");

            // Get user roles
            var roles = await _userManager.GetRolesAsync(identityUser);

            // Create and return the UserResponse DTO with roles
            var userResponse = new UserResponse
            {
                Id = identityUser.Id,
                Username = identityUser.UserName ?? string.Empty,
                Email = identityUser.Email ?? string.Empty,
                Roles = [.. roles],
            };

            return userResponse;
        }

        public async Task<UserResponse> PostUserRolesAsync(PostUserRolesDto dto)
        {
            // Ensure the user is authenticated
            var httpContext = _httpContextAccessor.HttpContext
                ?? throw new UnauthorizedAccessException("No active user session.");

            var currentUser = await _userManager.GetUserAsync(httpContext.User);
            // if (currentUser == null || !httpContext.User.IsInRole("Admin"))
            // {
            //     throw new UnauthorizedAccessException("You do not have permission to assign roles.");
            // }

            // Assign the role to the target user
            var user = await _userManager.FindByIdAsync(dto.UserId)
                ?? throw new NotFoundException($"User with ID '{dto.UserId}' not found.");
            
            foreach (var role in dto.Roles)
            {
                if (!await _userManager.IsInRoleAsync(user, role))
                {
                    var result = await _userManager.AddToRoleAsync(user, role);
                    if (!result.Succeeded)
                    {
                        throw new IdentityOperationException("Failed to assign role.", result.Errors);
                    }
                }
            }

            // Get user roles
            var roles = await _userManager.GetRolesAsync(user);

            // Create and return the UserResponse DTO with roles
            var userResponse = new UserResponse
            {
                Id = user.Id,
                Username = user.UserName ?? string.Empty,
                Email = user.Email ?? string.Empty,
                Roles = [.. roles],
            };

            return userResponse;
        }
    }
}