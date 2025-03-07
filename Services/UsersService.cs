using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RuminsterBackend.Data;
using RuminsterBackend.Exceptions;
using RuminsterBackend.Models;
using RuminsterBackend.Models.DTOs.User;
using RuminsterBackend.Services.Interfaces;

namespace RuminsterBackend.Services
{
    public class UsersService(
        UserManager<User> userManager,
        IHttpContextAccessor httpContextAccessor,
        RuminsterDbContext context
    ) : IUsersService
    {
        private readonly UserManager<User> _userManager = userManager;
        private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
        private readonly RuminsterDbContext _context = context;

        public async Task<UserResponse> GetCurrentUserAsync()
        {
            // Ensure the user is authenticated
            var httpContext = _httpContextAccessor.HttpContext
                ?? throw new UnauthorizedAccessException("No active user session.");

            var currentUser = httpContext.User;
            if (currentUser == null || !(currentUser.Identity?.IsAuthenticated ?? false))
            {
                throw new AuthenticationException("User not authenticated");
            }

            var user = await _userManager.GetUserAsync(currentUser) ??
                throw new NotFoundException("User not found");

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

        public async Task<List<UserResponse>> GetUsersAsync(GetUsersQueryParams queryParams)
        {
            var usersQuery = _context.Users
                .Include(q => q.UserRoles)
                    .ThenInclude(q => q.Role)
                .AsQueryable();

            if (queryParams.Username != null && queryParams.Username.Any(u => !string.IsNullOrEmpty(u)))
            {
                usersQuery = usersQuery.Where(u => queryParams.Username.Contains(u.UserName ?? string.Empty));
            }

            var users = await usersQuery.ToListAsync();

            var userResponses = users.Select(u => new UserResponse
            {
                Id = u.Id,
                Username = u.UserName ?? string.Empty,
                Email = u.Email ?? string.Empty,
                Roles = u.UserRoles.Select(r => r.RoleId).ToList(),
            }).ToList();

            return userResponses;
        }

        public async Task<UserResponse> PostUserRolesAsync(PostUserRolesDto dto)
        {
            var currentUser = await this.GetCurrentUserAsync();
            if (!currentUser?.Roles?.Contains("Admin") ?? true)
            {
                throw new UnauthorizedAccessException("You do not have permission to assign roles.");
            }

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