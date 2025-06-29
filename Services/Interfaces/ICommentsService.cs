using RuminsterBackend.Models.DTOs.Comment;

namespace RuminsterBackend.Services.Interfaces
{
    public interface ICommentsService
    {
        Task<List<CommentResponse>> GetCommentsByRuminationAsync(int ruminationId);
        Task<List<CommentResponse>> GetCommentRepliesAsync(int commentId);
        Task<CommentResponse> PostCommentAsync(PostCommentDto postCommentDto);
        Task<CommentResponse> PutCommentAsync(int commentId, PutCommentDto updateCommentDto);
        Task DeleteCommentAsync(int id);
    }
}
