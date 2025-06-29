namespace RuminsterBackend.Models.DTOs.Comment
{
    public class CommentResponse
    {
        public int Id { get; set; }
        public string Content { get; set; }
        public bool IsDeleted { get; set; }
        public int? RuminationId { get; set; }
        public int? ParentCommentId { get; set; }
        public DateTime CreateTMS { get; set; }
        public DateTime UpdateTMS { get; set; }
        public UserResponse CreatedBy { get; set; }
        public UserResponse UpdatedBy { get; set; }
        public List<CommentResponse> ChildComments { get; set; } = new();
    }

    public class UserResponse
    {
        public string Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string[] Roles { get; set; }
    }
}
