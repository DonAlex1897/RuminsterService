using Microsoft.EntityFrameworkCore;
using RuminsterBackend.Data;
using RuminsterBackend.Exceptions;
using RuminsterBackend.Models;
using RuminsterBackend.Models.DTOs.Comment;
using RuminsterBackend.Services.Interfaces;

namespace RuminsterBackend.Services
{
    public class CommentsService(
        IRequestContextService requestContextService
    ) : ICommentsService
    {
        private readonly IRequestContextService _requestContextService = requestContextService;
        private readonly DateTime _currentTime = requestContextService.Time;
        private readonly RuminsterDbContext _context = requestContextService.Context;

        public async Task<List<CommentResponse>> GetCommentsByRuminationAsync(int ruminationId)
        {
            var comments = await _context.Comments
                .Include(c => c.CreateBy)
                .Include(c => c.UpdateBy)
                .Include(c => c.ChildComments)
                    .ThenInclude(cc => cc.CreateBy)
                .Include(c => c.ChildComments)
                    .ThenInclude(cc => cc.UpdateBy)
                .Where(c => c.RuminationId == ruminationId && c.ParentCommentId == null)
                .OrderBy(c => c.CreateTMS)
                .AsNoTracking()
                .ToListAsync();

            return comments.Select(MapToResponse).ToList();
        }

        public async Task<List<CommentResponse>> GetCommentRepliesAsync(int commentId)
        {
            var replies = await _context.Comments
                .Include(c => c.CreateBy)
                .Include(c => c.UpdateBy)
                .Include(c => c.ChildComments)
                    .ThenInclude(cc => cc.CreateBy)
                .Include(c => c.ChildComments)
                    .ThenInclude(cc => cc.UpdateBy)
                .Where(c => c.ParentCommentId == commentId)
                .OrderBy(c => c.CreateTMS)
                .AsNoTracking()
                .ToListAsync();

            return replies.Select(MapToResponse).ToList();
        }

        public async Task<CommentResponse> PostCommentAsync(PostCommentDto postCommentDto)
        {
            var user = _requestContextService.User;

            // Validate that either RuminationId or ParentCommentId is provided, but not both
            if (postCommentDto.RuminationId == null && postCommentDto.ParentCommentId == null)
            {
                throw new BadRequestException("Either RuminationId or ParentCommentId must be provided");
            }

            if (postCommentDto.RuminationId != null && postCommentDto.ParentCommentId != null)
            {
                throw new BadRequestException("Cannot provide both RuminationId and ParentCommentId");
            }

            // If ParentCommentId is provided, get the RuminationId from the parent comment
            int ruminationId;
            if (postCommentDto.ParentCommentId != null)
            {
                var parentComment = await _context.Comments
                    .Where(c => c.Id == postCommentDto.ParentCommentId)
                    .FirstOrDefaultAsync();

                if (parentComment == null)
                {
                    throw new NotFoundException("Parent comment not found");
                }

                ruminationId = parentComment.RuminationId ?? throw new BadRequestException("Parent comment must be associated with a rumination");
            }
            else
            {
                ruminationId = postCommentDto.RuminationId!.Value;
            }

            // Verify the rumination exists
            var rumination = await _context.Ruminations
                .Where(r => r.Id == ruminationId)
                .FirstOrDefaultAsync();

            if (rumination == null)
            {
                throw new NotFoundException("Rumination not found");
            }

            var comment = new Comment
            {
                Content = postCommentDto.Content,
                RuminationId = ruminationId,
                ParentCommentId = postCommentDto.ParentCommentId,
                CreateById = user.Id,
                UpdateById = user.Id,
                CreateTMS = _currentTime,
                UpdateTMS = _currentTime
            };

            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();

            // Reload with navigation properties
            var savedComment = await _context.Comments
                .Include(c => c.CreateBy)
                .Include(c => c.UpdateBy)
                .Where(c => c.Id == comment.Id)
                .FirstAsync();

            return MapToResponse(savedComment);
        }

        public async Task<CommentResponse> PutCommentAsync(int commentId, PutCommentDto updateCommentDto)
        {
            var user = _requestContextService.User;

            var comment = await _context.Comments
                .Include(c => c.CreateBy)
                .Include(c => c.UpdateBy)
                .Where(c => c.Id == commentId && !c.IsDeleted)
                .FirstOrDefaultAsync() ?? throw new NotFoundException("Comment not found");

            // Only allow the creator to update their comment

            if (comment.CreateById != user.Id)
            {
                throw new ForbiddenException("You can only edit your own comments");
            }

            comment.Content = updateCommentDto.Content;
            comment.UpdateById = user.Id;
            comment.UpdateTMS = _currentTime;

            await _context.SaveChangesAsync();

            return MapToResponse(comment);
        }

        public async Task DeleteCommentAsync(int id)
        {
            var user = _requestContextService.User;

            var comment = await _context.Comments
                .Where(c => c.Id == id)
                .FirstOrDefaultAsync();

            if (comment == null)
            {
                throw new NotFoundException("Comment not found");
            }

            // Only allow the creator to delete their comment
            if (comment.CreateById != user.Id)
            {
                throw new ForbiddenException("You can only delete your own comments");
            }

            // Soft delete - mark as deleted but keep the record for child comments to remain visible
            comment.IsDeleted = true;
            comment.Content = "[This comment has been deleted]";
            comment.UpdateById = user.Id;
            comment.UpdateTMS = _currentTime;

            await _context.SaveChangesAsync();
        }

        private static CommentResponse MapToResponse(Comment comment)
        {
            return new CommentResponse
            {
                Id = comment.Id,
                Content = comment.Content,
                IsDeleted = comment.IsDeleted,
                RuminationId = comment.RuminationId,
                ParentCommentId = comment.ParentCommentId,
                CreateTMS = comment.CreateTMS,
                UpdateTMS = comment.UpdateTMS,
                CreatedBy = new UserResponse
                {
                    Id = comment.CreateBy.Id,
                    Username = comment.CreateBy.UserName!,
                    Email = comment.CreateBy.Email!,
                    Roles = comment.CreateBy.UserRoles?.Select(ur => ur.Role.Name!).ToArray() ?? Array.Empty<string>()
                },
                UpdatedBy = new UserResponse
                {
                    Id = comment.UpdateBy.Id,
                    Username = comment.UpdateBy.UserName!,
                    Email = comment.UpdateBy.Email!,
                    Roles = comment.UpdateBy.UserRoles?.Select(ur => ur.Role.Name!).ToArray() ?? Array.Empty<string>()
                },
                ChildComments = comment.ChildComments?.Select(MapToResponse).ToList() ?? new List<CommentResponse>()
            };
        }
    }
}
