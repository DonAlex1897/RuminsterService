using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RuminsterBackend.Exceptions;
using RuminsterBackend.Models;
using RuminsterBackend.Models.DTOs.Auth;
using RuminsterBackend.Models.DTOs.User;
using RuminsterBackend.Models.Enums;
using RuminsterBackend.Services.Interfaces;

namespace RuminsterBackend.Services
{
    public class AuthService(
        IRequestContextService contextService,
        UserManager<User> userManager,
        ITokenService tokenService) : IAuthService
    {
        private readonly IRequestContextService _contextService = contextService;
        private readonly UserManager<User> _userManager = userManager;
        private readonly ITokenService _tokenService = tokenService;

        public async Task<LoginResponse> SignUpAsync(PostSignUpDto dto)
        {
            // Check if the username or email already exists
            if (await _userManager.FindByNameAsync(dto.Username) != null)
                throw new ForbiddenException("Username is already taken.");

            if (await _userManager.FindByEmailAsync(dto.Email) != null)
                throw new ForbiddenException("Email is already registered.");

            // Create a new User
            var user = new User
            {
                UserName = dto.Username,
                Email = dto.Email
            };

            // Create the user with the provided password
            var result = await _userManager.CreateAsync(user, dto.Password);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new ForbiddenException($"User creation failed: {errors}");
            }

            // Generate access and refresh tokens
            var accessToken = _tokenService.GenerateAccessToken(user.Id, await _userManager.GetRolesAsync(user));
            var refreshToken = _tokenService.GenerateRefreshToken();
            var expiresIn = _tokenService.GetAccessTokenExpiry();

            // Update token in database
            var newRefreshTokenEntry = new RefreshToken
            {
                Token = refreshToken,
                UserId = user.Id,
                ExpiresAt = DateTime.UtcNow.AddDays(7)
            };

            await _contextService.Context.RefreshTokens.AddAsync(newRefreshTokenEntry);
            await _contextService.Context.SaveChangesAsync();

            if (!await _userManager.IsInRoleAsync(user, RoleType.User.ToString()))
            {
                var roleResult = await _userManager.AddToRoleAsync(user, RoleType.User.ToString());
                if (!roleResult.Succeeded)
                {
                    throw new IdentityOperationException("Failed to assign role.", roleResult.Errors);
                }
            }

            // Get user roles
            var roles = await _userManager.GetRolesAsync(user);
            
            // Return the login response
            return new LoginResponse
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                ExpiresIn = expiresIn,
                User = new UserResponse
                {
                    Id = user.Id,
                    Username = user.UserName!,
                    Email = user.Email,
                    Roles = [.. roles],
                }
            };
        }

        public async Task<LoginResponse> LoginAsync(PostLoginDto dto)
        {
            Console.WriteLine("checking username " + dto.Username);
            // Find the user by username
            var user = await _userManager.FindByNameAsync(dto.Username) ??
                throw new AuthenticationException("Invalid username or password.");

            Console.WriteLine("user id " + user.Id);

            Console.WriteLine("checking password " + dto.Password);
            // Check the password
            var isPasswordValid = await _userManager.CheckPasswordAsync(user, dto.Password);
            if (!isPasswordValid)
                throw new AuthenticationException("Invalid username or password.");

            Console.WriteLine("success");
            // Generate access and refresh tokens
            var accessToken = _tokenService.GenerateAccessToken(user.Id, await _userManager.GetRolesAsync(user));
            var refreshToken = _tokenService.GenerateRefreshToken();
            var expiresIn = _tokenService.GetAccessTokenExpiry();

            // Update token in database
            var newRefreshTokenEntry = new RefreshToken
            {
                Token = refreshToken,
                UserId = user.Id,
                ExpiresAt = DateTime.UtcNow.AddDays(7)
            };

            await _contextService.Context.RefreshTokens.AddAsync(newRefreshTokenEntry);
            await _contextService.Context.SaveChangesAsync();

            // Return the login response
            return new LoginResponse
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                ExpiresIn = expiresIn,
                User = new UserResponse
                {
                    Id = user.Id,
                    Username = user.UserName!,
                    Email = user.Email ?? string.Empty,
                }
            };
        }

        public async Task<TokenResponse> RefreshTokenAsync(PostRefreshTokenDto dto)
        {

            var user = await _userManager.FindByIdAsync(dto.UserId) ??
                throw new NotFoundException(dto.UserId + " is not a valid user id.");

            // Validate token
            var savedToken = await _contextService.Context.RefreshTokens
                .FirstOrDefaultAsync(t => t.Token == dto.RefreshToken && t.UserId == user.Id);
            if (savedToken == null || savedToken.ExpiresAt < DateTime.UtcNow || savedToken.IsRevoked)
                throw new AuthenticationException($"User ({user.UserName}) is not authenticated.");

            // Generate new tokens
            var newAccessToken = _tokenService.GenerateAccessToken(user.Id, await _userManager.GetRolesAsync(user));
            var newRefreshToken = _tokenService.GenerateRefreshToken();

            // Update token in database
            var newRefreshTokenEntry = new RefreshToken
            {
                Token = newRefreshToken,
                UserId = user.Id,
                ExpiresAt = DateTime.UtcNow.AddDays(7)
            };

            savedToken.IsRevoked = true;
            _contextService.Context.RefreshTokens.Update(savedToken);
            await _contextService.Context.RefreshTokens.AddAsync(newRefreshTokenEntry);
            await _contextService.Context.SaveChangesAsync();

            return new TokenResponse
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken
            };
        }
    }
}