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
        ITokenService tokenService,
        IEmailService emailService,
        ITermsOfServiceService termsOfServiceService) : IAuthService
    {
        private readonly IRequestContextService _contextService = contextService;
        private readonly UserManager<User> _userManager = userManager;
        private readonly ITokenService _tokenService = tokenService;
        private readonly IEmailService _emailService = emailService;
        private readonly ITermsOfServiceService _termsOfServiceService = termsOfServiceService;

        public async Task<LoginResponse> LoginAsync(PostLoginDto dto)
        {
            // Find the user by username or email
            var user = await _userManager.FindByNameAsync(dto.Username) ??
                       await _userManager.FindByEmailAsync(dto.Username) ??
                       throw new AuthenticationException("Invalid username or password.");

            // Check if email is confirmed
            if (!await _userManager.IsEmailConfirmedAsync(user))
                throw new AuthenticationException("Please confirm your email address before logging in.");

            // Check the password
            var isPasswordValid = await _userManager.CheckPasswordAsync(user, dto.Password);
            if (!isPasswordValid)
                throw new AuthenticationException("Invalid username or password.");

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

            // Check TOS acceptance status
            var hasAcceptedLatestTos = await _termsOfServiceService.HasUserAcceptedLatestTosAsync(user.Id);
            var latestTos = await _termsOfServiceService.GetActiveTermsOfServiceAsync();

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
                },
                RequiresTosAcceptance = !hasAcceptedLatestTos,
                LatestTosVersion = latestTos?.Version
            };
        }

        public async Task<TokenResponse> RefreshTokenAsync(PostRefreshTokenDto dto)
        {
            // Validate token - first find the refresh token to get the user ID
            var savedToken = await _contextService.Context.RefreshTokens
                .FirstOrDefaultAsync(t => t.Token == dto.RefreshToken);
            if (savedToken == null || savedToken.ExpiresAt < DateTime.UtcNow || savedToken.IsRevoked)
                throw new AuthenticationException("Invalid or expired refresh token.");

            var userId = savedToken.UserId;

            // Get the user
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                throw new AuthenticationException("User not found.");

            // Generate new tokens
            var newAccessToken = _tokenService.GenerateAccessToken(userId, await _userManager.GetRolesAsync(user));
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
                RefreshToken = newRefreshToken,
                ExpiresIn = _tokenService.GetAccessTokenExpiry()
            };
        }

        public async Task<string> SignUpAsync(PostSignUpDto dto)
        {
            // Check if the username or email already exists
            if (await _userManager.FindByNameAsync(dto.Username) != null)
                throw new ForbiddenException("Username is already taken.");

            if (await _userManager.FindByEmailAsync(dto.Email) != null)
                throw new ForbiddenException("Email is already registered.");

            // Create a new User (unconfirmed)
            var user = new User
            {
                UserName = dto.Username,
                Email = dto.Email,
                EmailConfirmed = false
            };

            // Create the user with the provided password
            var result = await _userManager.CreateAsync(user, dto.Password);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new ForbiddenException($"User creation failed: {errors}");
            }

            // Assign default role
            if (!await _userManager.IsInRoleAsync(user, RoleType.User.ToString()))
            {
                var roleResult = await _userManager.AddToRoleAsync(user, RoleType.User.ToString());
                if (!roleResult.Succeeded)
                {
                    throw new IdentityOperationException("Failed to assign role.", roleResult.Errors);
                }
            }

            // Generate email verification token
            var token = GenerateSecureToken();
            var userToken = new UserToken
            {
                UserId = user.Id,
                Token = token,
                TokenType = UserTokenType.EmailVerification,
                ExpiresAt = DateTime.UtcNow.AddMinutes(30),
                CreatedAt = DateTime.UtcNow,
                IsUsed = false
            };

            await _contextService.Context.UserTokens.AddAsync(userToken);
            await _contextService.Context.SaveChangesAsync();

            // Record TOS acceptance
            var activeTos = await _contextService.Context.TermsOfService
                .Where(tos => tos.IsActive && tos.Version == dto.AcceptedTosVersion)
                .FirstOrDefaultAsync();

            if (activeTos != null)
            {
                var tosAcceptance = new UserTosAcceptance
                {
                    UserId = user.Id,
                    AcceptedVersion = dto.AcceptedTosVersion,
                    AcceptedAt = DateTime.UtcNow,
                    TermsOfServiceId = activeTos.Id
                };

                await _contextService.Context.UserTosAcceptances.AddAsync(tosAcceptance);
                await _contextService.Context.SaveChangesAsync();
            }

            // Send activation email
            await _emailService.SendEmailVerificationAsync(user.Email!, user.UserName!, token);

            return "Registration successful! Please check your email to activate your account.";
        }

        public async Task<string> ActivateAccountAsync(GetActivateDto dto)
        {
            // Find the token
            var userToken = await _contextService.Context.UserTokens
                .Include(t => t.User)
                .FirstOrDefaultAsync(t => t.Token == dto.Token && 
                                          t.TokenType == UserTokenType.EmailVerification && 
                                          !t.IsUsed);

            if (userToken == null)
                throw new NotFoundException("Invalid or expired activation token.");

            if (userToken.ExpiresAt < DateTime.UtcNow)
                throw new ForbiddenException("Activation token has expired.");

            var user = userToken.User;
            if (user == null)
                throw new NotFoundException("User not found.");

            // Confirm the user's email
            user.EmailConfirmed = true;
            userToken.IsUsed = true;

            _contextService.Context.Users.Update(user);
            _contextService.Context.UserTokens.Update(userToken);
            await _contextService.Context.SaveChangesAsync();

            return "Account activated successfully! You can now log in.";
        }

        public async Task<string> SendPasswordResetEmailAsync(PostForgotPasswordDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null)
            {
                // Don't reveal if email exists - return success message anyway for security
                return "If an account with that email exists, a password reset link has been sent.";
            }

            // Generate password reset token
            var token = GenerateSecureToken();
            var userToken = new UserToken
            {
                UserId = user.Id,
                Token = token,
                TokenType = UserTokenType.PasswordReset,
                ExpiresAt = DateTime.UtcNow.AddMinutes(15),
                CreatedAt = DateTime.UtcNow,
                IsUsed = false
            };

            await _contextService.Context.UserTokens.AddAsync(userToken);
            await _contextService.Context.SaveChangesAsync();

            // Send password reset email
            await _emailService.SendPasswordResetAsync(user.Email!, user.UserName!, token);

            return "If an account with that email exists, a password reset link has been sent.";
        }

        public async Task<string> ResetPasswordAsync(PostResetPasswordDto dto)
        {
            // Find the token
            var userToken = await _contextService.Context.UserTokens
                .Include(t => t.User)
                .FirstOrDefaultAsync(t => t.Token == dto.Token && 
                                          t.TokenType == UserTokenType.PasswordReset && 
                                          !t.IsUsed);

            if (userToken == null)
                throw new NotFoundException("Invalid or expired reset token.");

            if (userToken.ExpiresAt < DateTime.UtcNow)
                throw new ForbiddenException("Reset token has expired.");

            var user = userToken.User;
            if (user == null)
                throw new NotFoundException("User not found.");

            // Reset the password
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, token, dto.NewPassword);
            
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new ForbiddenException($"Password reset failed: {errors}");
            }

            // Mark token as used
            userToken.IsUsed = true;
            _contextService.Context.UserTokens.Update(userToken);
            
            // Revoke all existing refresh tokens for security
            var refreshTokens = await _contextService.Context.RefreshTokens
                .Where(rt => rt.UserId == user.Id && !rt.IsRevoked)
                .ToListAsync();
            
            foreach (var refreshToken in refreshTokens)
            {
                refreshToken.IsRevoked = true;
            }
            
            _contextService.Context.RefreshTokens.UpdateRange(refreshTokens);
            await _contextService.Context.SaveChangesAsync();

            return "Password reset successful! You can now log in with your new password.";
        }

        private static string GenerateSecureToken()
        {
            return Guid.NewGuid().ToString("N") + Guid.NewGuid().ToString("N");
        }
    }
}