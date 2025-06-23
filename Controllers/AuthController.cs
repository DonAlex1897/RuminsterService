using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RuminsterBackend.Models.DTOs.Auth;
using RuminsterBackend.Models.DTOs.User;
using RuminsterBackend.Services.Interfaces;

namespace RuminsterBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController(
        IRequestContextService contextService,
        IAuthService authService,
        IUsersService usersService) : ControllerBase
    {
        private readonly IRequestContextService _contextService = contextService;
        private readonly IAuthService _authService = authService;
        private readonly IUsersService _usersService = usersService;

        [HttpGet("me")]
        public async Task<ActionResult<UserResponse>> GetCurrentUserAsync()
        {
            var response = await _usersService.GetCurrentUserAsync();
            return Ok(response);
        }

        [HttpPost("signup")]
        public async Task<ActionResult<LoginResponse>> SignUp([FromBody] PostSignUpDto dto)
        {
            using var transaction = await _contextService.Context.Database.BeginTransactionAsync();
            var response = await _authService.SignUpAsync(dto);
            await transaction.CommitAsync();
            return Ok(response);
        }

        [HttpPost("login")]
        public async Task<ActionResult<LoginResponse>> Login([FromBody] PostLoginDto dto)
        {
            using var transaction = await _contextService.Context.Database.BeginTransactionAsync();
            var response = await _authService.LoginAsync(dto);
            await transaction.CommitAsync();
            return Ok(response);
        }

        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshTokenAsync([FromBody] PostRefreshTokenDto dto)
        {
            using var transaction = await _contextService.Context.Database.BeginTransactionAsync();
            var response = await _authService.RefreshTokenAsync(dto);
            await transaction.CommitAsync();
            return Ok(response);
        }
    }
}