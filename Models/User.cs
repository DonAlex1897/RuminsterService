using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace RuminsterBackend.Models
{
    public class User : IdentityUser
    {
        public string? Name { get; set; }

        public virtual ICollection<UserRole> UserRoles { get; set; }

        public ICollection<Rumination> RuminationsCreateBy { get; set; }

        public ICollection<Rumination> RuminationsUpdateBy { get; set; }

        public ICollection<RuminationLog> RuminationLogsCreateBy { get; set; }

        public ICollection<UserRelation> UserRelationsInitiator { get; set; }

        public ICollection<UserRelation> UserRelationsReceiver { get; set; }

        public ICollection<UserRelation> UserRelationsCreateBy { get; set; }

        public ICollection<UserRelation> UserRelationsUpdateBy { get; set; }

        public ICollection<UserRelationLog> UserRelationLogsInitiator { get; set; }

        public ICollection<UserRelationLog> UserRelationLogsReceiver { get; set; }

        public ICollection<UserRelationLog> UserRelationLogsCreateBy { get; set; }

        public ICollection<RefreshToken> RefreshTokens { get; set; }

        public ICollection<UserToken> UserTokens { get; set; }

        public ICollection<UserTosAcceptance> TosAcceptances { get; set; }

        public ICollection<Comment> CommentsCreateBy { get; set; }

        public ICollection<Comment> CommentsUpdateBy { get; set; }
    }
}