using Microsoft.AspNetCore.Identity;
using RuminsterBackend.Models.Enums;

namespace RuminsterBackend.Models
{
    public class UserRelation
    {
        public int Id { get; set; }

        public bool IsDeleted { get; set; }

        public string InitiatorId { get; set; }

        public IdentityUser Initiator { get; set; }

        public string ReceiverId { get; set; }

        public IdentityUser Receiver { get; set; }

        public bool IsAccepted { get; set; }

        public bool IsRejected { get; set; }

        public UserRelationType Type { get; set; }

        public string CreateById { get; set; }

        public IdentityUser CreateBy { get; set; }

        public string UpdateById { get; set; }

        public IdentityUser UpdateBy { get; set; }

        public DateTime CreateTMS { get; set; }

        public DateTime UpdateTMS { get; set; }

        public ICollection<UserRelationLog> Logs { get; set; }
    }
}