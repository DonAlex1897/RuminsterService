using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RuminsterBackend.Models.DTOs.User;
using RuminsterBackend.Models.Enums;

namespace RuminsterBackend.Models.DTOs.UserRelation
{
    public class UserRelationResponse
    {
        public int Id { get; set; }

        public UserResponse Initiator { get; set; }

        public UserResponse Receiver { get; set; }

        public bool IsAccepted { get; set; }

        public bool IsRejected { get; set; }

        public UserRelationType Type { get; set; }
    }
}