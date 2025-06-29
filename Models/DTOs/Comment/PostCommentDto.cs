namespace RuminsterBackend.Models.DTOs.Comment
{
    public class PostCommentDto
    {
        public string Content { get; set; }
        public int? RuminationId { get; set; }
        public int? ParentCommentId { get; set; }
    }
}
