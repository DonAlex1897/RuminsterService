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

        [HttpGet]
        public async Task<ActionResult<List<UserResponse>>> GetUsersAsync([FromQuery] GetUsersQueryParams queryParams)
        {
            var response = await _usersService.GetUsersAsync(queryParams);
            return Ok(response);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<UserResponse>> GetUserById(string id)
        {
            var user = await _usersService.GetUserByIdAsync(id);
            return Ok(user);
        }

        [Authorize(Roles = "Admin")]
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