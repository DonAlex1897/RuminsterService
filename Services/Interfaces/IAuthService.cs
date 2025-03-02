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
        Task<LoginResponse> SignUpAsync(PostSignUpDto dto);
        
        Task<LoginResponse> LoginAsync(PostLoginDto dto);

        Task<TokenResponse> RefreshTokenAsync(PostRefreshTokenDto dto);
    }
}