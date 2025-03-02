
namespace RuminsterBackend.Models.DTOs.UserRelation
{
    using RuminsterBackend.Models;
    using RuminsterBackend.Models.DTOs.User;

    public static class UserRelationMapper
    {
        public static UserRelationResponse MapUserRelationResponse(UserRelation userRelation)
        {
            return new UserRelationResponse
            {
                Id = userRelation.Id,
                Initiator = UserMapper.MapUserResponse(userRelation.Initiator),
                Receiver = UserMapper.MapUserResponse(userRelation.Receiver),
                IsAccepted = userRelation.IsAccepted,
                IsRejected = userRelation.IsRejected,
                Type = userRelation.Type,
            };
        }
    }
}