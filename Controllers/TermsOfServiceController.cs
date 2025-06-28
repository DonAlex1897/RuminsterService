using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RuminsterBackend.Services.Interfaces;

namespace RuminsterBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TermsOfServiceController : ControllerBase
    {
        private readonly ITermsOfServiceService _tosService;
        private readonly IRequestContextService _contextService;

        public TermsOfServiceController(
            ITermsOfServiceService tosService,
            IRequestContextService contextService)
        {
            _tosService = tosService;
            _contextService = contextService;
        }

        [HttpGet("current")]
        public async Task<IActionResult> GetCurrentTermsOfServiceAsync()
        {
            var tos = await _tosService.GetActiveTermsOfServiceAsync();
            if (tos == null)
                return NotFound("No active Terms of Service found");

            return Ok(new
            {
                version = tos.Version,
                content = tos.Content,
                createdAt = tos.CreatedAt
            });
        }

        [HttpPost("accept")]
        [Authorize]
        public async Task<IActionResult> AcceptTermsOfServiceAsync([FromBody] AcceptTosRequest request)
        {
            var userId = _contextService.User.Id;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            try
            {
                await _tosService.RecordTosAcceptanceAsync(userId, request.Version);
                return Ok(new { message = "Terms of Service accepted successfully" });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("acceptance-status")]
        [Authorize]
        public async Task<IActionResult> GetAcceptanceStatusAsync()
        {
            var userId = _contextService.User.Id;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var hasAccepted = await _tosService.HasUserAcceptedLatestTosAsync(userId);
            var currentTos = await _tosService.GetActiveTermsOfServiceAsync();

            return Ok(new
            {
                hasAcceptedLatest = hasAccepted,
                currentVersion = currentTos?.Version,
                requiresAcceptance = !hasAccepted && currentTos != null
            });
        }

        [HttpPost("create")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateNewTermsOfServiceAsync([FromBody] CreateTosRequest request)
        {
            try
            {
                await _tosService.CreateNewTermsOfServiceAsync(request.Version, request.Content);
                return Ok(new { message = "New Terms of Service created successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("pending-users")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetUsersWhoNeedToAcceptAsync()
        {
            var userIds = await _tosService.GetUsersWhoNeedToAcceptLatestTosAsync();
            return Ok(new { userIds, count = userIds.Count });
        }
    }

    public class AcceptTosRequest
    {
        public string Version { get; set; } = "";
    }

    public class CreateTosRequest
    {
        public string Version { get; set; } = "";
        public string Content { get; set; } = "";
    }
}
