using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RuminsterBackend.Models.DTOs.Auth;
using RuminsterBackend.Models.DTOs.User;
using RuminsterBackend.Services.Interfaces;

namespace RuminsterBackend.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController(
        IRequestContextService contextService,
        IUsersService usersService) : ControllerBase
    {
        private readonly IRequestContextService _contextService = contextService;
        private readonly IUsersService _usersService = usersService;

        [HttpGet("current")]
        public async Task<ActionResult<UserResponse>> GetCurrentUserAsync()
        {
            using var transaction = await _contextService.Context.Database.BeginTransactionAsync();
            var response = await _usersService.GetCurrentUserAsync();
            await transaction.CommitAsync();
            return Ok(response);
        }

        // [Authorize(Roles = "Admin")]
        [HttpPost("UserRoles")]
        public async Task<ActionResult<UserResponse>> PostUserRolesAsync([FromBody] PostUserRolesDto dto)
        {
            using var transaction = await _contextService.Context.Database.BeginTransactionAsync();
            var response = await _usersService.PostUserRolesAsync(dto);
            await transaction.CommitAsync();
            return Ok(response);
        }
    }
}