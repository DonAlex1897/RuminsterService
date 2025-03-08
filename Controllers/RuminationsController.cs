using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RuminsterBackend.Models.DTOs.Auth;
using RuminsterBackend.Models.DTOs.Rumination;
using RuminsterBackend.Models.DTOs.User;
using RuminsterBackend.Services.Interfaces;

namespace RuminsterBackend.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class RuminationsController(
        IRequestContextService requestContextService,
        IRuminationsService ruminationsService) : ControllerBase
    {
        private readonly IRequestContextService _requestContextService = requestContextService;
        private readonly IRuminationsService _ruminationsService = ruminationsService;

        [HttpGet("public")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<List<RuminationResponse>>> GetPublicRuminationsAsync([FromQuery]GetRuminationsQueryParams queryParams)
        {
            var response = await _ruminationsService.GetPublicRuminationsAsync(queryParams);
            return Ok(response);
        }
        
        [HttpGet("myRuminations")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<List<RuminationResponse>>> GetMyRuminationsAsync([FromQuery]GetMyRuminationsQueryParams queryParams)
        {
            var response = await _ruminationsService.GetMyRuminationsAsync(queryParams);
            return Ok(response);
        }
        
        [HttpGet("{ruminationId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<RuminationResponse>> GetRuminationAsync(int ruminationId)
        {
            var response = await _ruminationsService.GetRuminationAsync(ruminationId);
            return Ok(response);
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<RuminationResponse>> PostRuminationAsync([FromBody] PostRuminationDto dto)
        {
            using var transaction = await _requestContextService.Context.Database.BeginTransactionAsync();
            var response = await _ruminationsService.PostRuminationAsync(dto);
            await transaction.CommitAsync();
            return this.StatusCode(201, response);
        }

        [HttpPut("{ruminationId}/content")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<RuminationResponse>> PutRuminationContentAsync(int ruminationId, PutRuminationContentDto dto)
        {
            using var transaction = await _requestContextService.Context.Database.BeginTransactionAsync();
            var response = await _ruminationsService.PutRuminationContentAsync(ruminationId, dto);
            await transaction.CommitAsync();
            return this.Ok(response);
        }

        [HttpPut("{ruminationId}/visibility")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<RuminationResponse>> PutRuminationVisibilityAsync(int ruminationId)
        {
            using var transaction = await _requestContextService.Context.Database.BeginTransactionAsync();
            var response = await _ruminationsService.PutRuminationVisibilityAsync(ruminationId);
            await transaction.CommitAsync();
            return this.Ok(response);
        }

        [HttpDelete("{ruminationId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> DeletePurchaseOrderAsync(int ruminationId)
        {
            using var transaction = await _requestContextService.Context.Database.BeginTransactionAsync();
            await _ruminationsService.DeleteRuminationAsync(ruminationId);
            await transaction.CommitAsync();

            return this.NoContent();
        }

        [HttpPut("{ruminationId}/audiences")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<RuminationResponse>> PutRuminationAudiencesAsync(int ruminationId, PutRuminationAudiencesDto dto)
        {
            using var transaction = await _requestContextService.Context.Database.BeginTransactionAsync();
            var response = await _ruminationsService.PutRuminationAudiencesAsync(ruminationId, dto);
            await transaction.CommitAsync();
            return this.Ok(response);
        }
    }
}