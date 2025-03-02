using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RuminsterBackend.Models.DTOs.UserRelation;

namespace RuminsterBackend.Services.Interfaces
{
    public interface IUserRelationsService
    {
        Task<List<UserRelationResponse>> GetUserRelationsAsync();

        Task<UserRelationResponse> GetUserRelationAsync(int userRelationId);

        Task<UserRelationResponse> PostUserRelationAsync(PostUserRelationDto dto);

        Task<UserRelationResponse> PutUserRelationAcceptAsync(int userRelationId);

        Task<UserRelationResponse> PutUserRelationRejectAsync(int userRelationId);

        Task DeleteUserRelationAsync(int userRelationId);
    }
}