using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RuminsterBackend.Models.DTOs.UserRelation;
using RuminsterBackend.Services.Interfaces;

namespace RuminsterBackend.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class UserRelationsController(
        IRequestContextService requestContextService,
        IUserRelationsService userRelationsService) : ControllerBase
    {
        private readonly IRequestContextService _requestContextService = requestContextService;
        private readonly IUserRelationsService _userRelationsService = userRelationsService;

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<List<UserRelationResponse>>> GetUserRelationsAsync([FromQuery] GetUserRelationsQueryParams queryParams)
        {
            var response = await _userRelationsService.GetUserRelationsAsync(queryParams);
            return Ok(response);
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<UserRelationResponse>> PostUserRelationAsync([FromBody] PostUserRelationDto dto)
        {
            using var transaction = await _requestContextService.Context.Database.BeginTransactionAsync();
            var response = await _userRelationsService.PostUserRelationAsync(dto);
            await transaction.CommitAsync();
            return this.StatusCode(201, response);
        }

        [HttpPut("{userRelationId}/accept")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<UserRelationResponse>> PutUserRelationAcceptAsync(int userRelationId)
        {
            using var transaction = await _requestContextService.Context.Database.BeginTransactionAsync();
            var response = await _userRelationsService.PutUserRelationAcceptAsync(userRelationId);
            await transaction.CommitAsync();
            return this.Ok(response);
        }

        [HttpPut("{userRelationId}/reject")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<UserRelationResponse>> PutUserRelationRejectAsync(int userRelationId)
        {
            using var transaction = await _requestContextService.Context.Database.BeginTransactionAsync();
            var response = await _userRelationsService.PutUserRelationRejectAsync(userRelationId);
            await transaction.CommitAsync();
            return this.Ok(response);
        }

        [HttpDelete("{userRelationId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> DeleteUserRelationAsync(int userRelationId)
        {
            using var transaction = await _requestContextService.Context.Database.BeginTransactionAsync();
            await _userRelationsService.DeleteUserRelationAsync(userRelationId);
            await transaction.CommitAsync();

            return this.NoContent();
        }
    }
}