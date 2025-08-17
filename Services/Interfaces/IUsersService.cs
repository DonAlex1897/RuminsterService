using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RuminsterBackend.Models.DTOs.User;

namespace RuminsterBackend.Services.Interfaces
{
    public interface IUsersService
    {
        Task<UserResponse> GetCurrentUserAsync();

        Task<List<UserResponse>> GetUsersAsync(GetUsersQueryParams queryParams);

        Task<UserResponse> GetUserByIdAsync(string userId);

        Task<UserResponse> PostUserRolesAsync(PostUserRolesDto dto);

        Task<UserResponse> PutUserNameAsync(PutUserNameDto dto);

        Task<List<UserResponse>> SearchUsersAsync(string query, int? limit = 10, int? offset = null);
    }
}