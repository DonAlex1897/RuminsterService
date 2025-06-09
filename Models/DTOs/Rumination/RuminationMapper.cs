namespace RuminsterBackend.Models.DTOs.Rumination
{
    using RuminsterBackend.Models;
    using RuminsterBackend.Models.DTOs.User;

    public static class RuminationMapper
    {
        public static RuminationResponse MapRuminationResponse(Rumination rumination)
        {
            return new RuminationResponse
            {
                Id = rumination.Id,
                Content = rumination.Content,
                IsPublished = rumination.IsPublished,
                Audiences = [..rumination.Audiences?
                    .Where(q => !q.IsDeleted)?
                    .Select(MapRuminationAudienceResponse) ?? []],
                CreatedBy = UserMapper.MapUserResponse(rumination.CreateBy),
                CreateTMS = rumination.CreateTMS,
                UpdatedBy = UserMapper.MapUserResponse(rumination.UpdateBy),
                UpdateTMS = rumination.UpdateTMS,
            };
        }

        public static RuminationAudienceResponse MapRuminationAudienceResponse(RuminationAudience ruminationAudience)
        {
            return new RuminationAudienceResponse
            {
                Id = ruminationAudience.Id,
                RelationType = ruminationAudience.RelationType,
                CreateTMS = ruminationAudience.CreateTMS,
                UpdateTMS = ruminationAudience.UpdateTMS,
            };
        }
    }
}