using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RuminsterBackend.Models.DTOs.Rumination;
using RuminsterBackend.Models.DTOs.User;

namespace RuminsterBackend.Services.Interfaces
{
    public interface IRuminationsService
    {
        Task<List<RuminationResponse>> GetPublicRuminationsAsync(GetRuminationsQueryParams queryParams);

        Task<List<RuminationResponse>> GetMyRuminationsAsync(GetMyRuminationsQueryParams queryParams);

        Task<RuminationResponse> GetRuminationAsync(int ruminationId);

        Task<RuminationResponse> PostRuminationAsync(PostRuminationDto dto);

        Task<RuminationResponse> PutRuminationContentAsync(int ruminationId, PutRuminationContentDto dto);

        Task<RuminationResponse> PutRuminationVisibilityAsync(int ruminationId);

        Task DeleteRuminationAsync(int ruminationId);
        
        Task<RuminationResponse> PutRuminationAudiencesAsync(int ruminationId, PutRuminationAudiencesDto dto);
    }
}