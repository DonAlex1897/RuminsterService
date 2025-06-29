using Microsoft.AspNetCore.Identity;

namespace RuminsterBackend.Models
{
    public class Comment
    {
        public int Id { get; set; }

        public bool IsDeleted { get; set; }

        public string Content { get; set; }

        public int? RuminationId { get; set; }

        public Rumination? Rumination { get; set; }

        public int? ParentCommentId { get; set; }

        public Comment? ParentComment { get; set; }

        public string CreateById { get; set; }

        public User CreateBy { get; set; }

        public string UpdateById { get; set; }

        public User UpdateBy { get; set; }

        public DateTime CreateTMS { get; set; }

        public DateTime UpdateTMS { get; set; }

        public ICollection<Comment> ChildComments { get; set; }
    }
}
