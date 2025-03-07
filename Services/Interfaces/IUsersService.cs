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

        Task<UserResponse> PostUserRolesAsync(PostUserRolesDto dto);
    }
}