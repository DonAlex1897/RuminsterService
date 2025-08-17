using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RuminsterBackend.Models.DTOs.Rumination;
using RuminsterBackend.Models.DTOs.Search;
using RuminsterBackend.Services.Interfaces;

namespace RuminsterBackend.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class SearchController(
        IRuminationsService ruminationsService,
        IUsersService usersService
    ) : ControllerBase
    {
        private readonly IRuminationsService _ruminationsService = ruminationsService;
        private readonly IUsersService _usersService = usersService;

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<SearchResponse>> SearchAsync([FromQuery] SearchQueryParams queryParams)
        {
            var searchText = (queryParams.Query ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(searchText))
            {
                return Ok(new SearchResponse());
            }

            // Run sequentially to avoid concurrent use of the same DbContext instance across services
            var ruminations = await _ruminationsService.SearchAccessibleRuminationsAsync(
                searchText,
                Math.Min(queryParams.RuminationsLimit ?? 10, 50),
                queryParams.RuminationsOffset
            );

            var users = await _usersService.SearchUsersAsync(
                searchText,
                Math.Min(queryParams.UsersLimit ?? 10, 50),
                queryParams.UsersOffset
            );

            var response = new SearchResponse
            {
                Ruminations = ruminations,
                Users = users,
            };

            return Ok(response);
        }
    }
}
