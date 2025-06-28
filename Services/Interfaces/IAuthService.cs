using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RuminsterBackend.Models.DTOs.Auth;
using RuminsterBackend.Models.DTOs.User;

namespace RuminsterBackend.Services.Interfaces
{
    public interface IAuthService
    {
        Task<LoginResponse> LoginAsync(PostLoginDto dto);

        Task<TokenResponse> RefreshTokenAsync(PostRefreshTokenDto dto);

        Task<string> SignUpAsync(PostSignUpDto dto);
        
        Task<string> ActivateAccountAsync(GetActivateDto dto);
        
        Task<string> SendPasswordResetEmailAsync(PostForgotPasswordDto dto);
        
        Task<string> ResetPasswordAsync(PostResetPasswordDto dto);
    }
}