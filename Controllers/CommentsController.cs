using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RuminsterBackend.Models.DTOs.Comment;
using RuminsterBackend.Services.Interfaces;

namespace RuminsterBackend.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class CommentsController(
        IRequestContextService requestContextService,
        ICommentsService commentsService) : ControllerBase
    {
        private readonly IRequestContextService _requestContextService = requestContextService;
        private readonly ICommentsService _commentsService = commentsService;

        [HttpGet("rumination/{ruminationId}")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<List<CommentResponse>>> GetCommentsByRuminationAsync(int ruminationId)
        {
            var response = await _commentsService.GetCommentsByRuminationAsync(ruminationId);
            return Ok(response);
        }

        [HttpGet("comment/{commentId}/replies")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<List<CommentResponse>>> GetCommentRepliesAsync(int commentId)
        {
            var response = await _commentsService.GetCommentRepliesAsync(commentId);
            return Ok(response);
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<CommentResponse>> PostCommentAsync([FromBody] PostCommentDto postCommentDto)
        {
            using var transaction = await _requestContextService.Context.Database.BeginTransactionAsync();
            var response = await _commentsService.PostCommentAsync(postCommentDto);
            await transaction.CommitAsync();
            return Ok(response);
        }

        [HttpPut("{commentId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<CommentResponse>> PutCommentAsync(int commentId, [FromBody] PutCommentDto updateCommentDto)
        {
            using var transaction = await _requestContextService.Context.Database.BeginTransactionAsync();
            var response = await _commentsService.PutCommentAsync(commentId, updateCommentDto);
            await transaction.CommitAsync();
            return Ok(response);
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> DeleteCommentAsync(int id)
        {
            using var transaction = await _requestContextService.Context.Database.BeginTransactionAsync();
            await _commentsService.DeleteCommentAsync(id);
            await transaction.CommitAsync();
            return NoContent();
        }
    }
}
