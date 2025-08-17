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
        RuminsterDbContext context,
        ITextSearchService textSearchService
    ) : IUsersService
    {
        private readonly UserManager<User> _userManager = userManager;
        private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
        private readonly RuminsterDbContext _context = context;
        private readonly ITextSearchService _textSearchService = textSearchService;

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
                Name = user.Name,
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

            var userResponses = users.Select(UserMapper.MapUserResponse).ToList();

            return userResponses;
        }

        public async Task<UserResponse> GetUserByIdAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId)
                ?? throw new NotFoundException($"User with ID '{userId}' not found.");

            // Get user roles
            var roles = await _userManager.GetRolesAsync(user);

            // Create and return the UserResponse DTO with roles
            var userResponse = new UserResponse
            {
                Id = user.Id,
                Username = user.UserName ?? string.Empty,
                Email = user.Email ?? string.Empty,
                Name = user.Name,
                Roles = [.. roles],
            };

            return userResponse;
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
                Name = user.Name,
                Roles = [.. roles],
            };

            return userResponse;
        }

        public async Task<UserResponse> PutUserNameAsync(PutUserNameDto dto)
        {
            var httpContext = _httpContextAccessor.HttpContext
                ?? throw new UnauthorizedAccessException("No active user session.");

            var currentUser = httpContext.User;
            if (currentUser == null || !(currentUser.Identity?.IsAuthenticated ?? false))
            {
                throw new AuthenticationException("User not authenticated");
            }

            var user = await _userManager.GetUserAsync(currentUser)
                ?? throw new NotFoundException("User not found");

            var newName = dto.Name?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(newName))
            {
                throw new ValidationException("Name cannot be empty.");
            }

            // Optional: basic length guard; database uses text so no strict limit, but cap to something reasonable
            if (newName.Length > 200)
            {
                throw new ValidationException("Name is too long (max 200 characters).");
            }

            user.Name = newName;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                throw new IdentityOperationException("Failed to update user name.", result.Errors);
            }

            var roles = await _userManager.GetRolesAsync(user);

            return new UserResponse
            {
                Id = user.Id,
                Username = user.UserName ?? string.Empty,
                Email = user.Email ?? string.Empty,
                Name = user.Name,
                Roles = [.. roles],
            };
        }

        public async Task<List<UserResponse>> SearchUsersAsync(string query, int? limit = 10, int? offset = null)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return new List<UserResponse>();
            }

            var usersBase = _context.Users
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .AsQueryable();

            var usersFiltered = _textSearchService.ApplyContainsFilter(usersBase, query, u => u.UserName!, u => u.Name);
            // Ensure deterministic ordering before Skip/Take to avoid EF warning and unstable pages
            usersFiltered = usersFiltered
                .OrderBy(u => u.UserName)
                .ThenBy(u => u.Id);
            usersFiltered = _textSearchService.ApplyPagination(usersFiltered, offset, limit, 50);

            var users = await usersFiltered.ToListAsync();
            return users.Select(UserMapper.MapUserResponse).ToList();
        }
    }
}