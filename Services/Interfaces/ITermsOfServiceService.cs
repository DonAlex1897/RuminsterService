using RuminsterBackend.Models;

namespace RuminsterBackend.Services.Interfaces
{
    public interface ITermsOfServiceService
    {
        Task<TermsOfService?> GetActiveTermsOfServiceAsync();
        Task<bool> HasUserAcceptedLatestTosAsync(string userId);
        Task RecordTosAcceptanceAsync(string userId, string tosVersion);
        Task<List<string>> GetUsersWhoNeedToAcceptLatestTosAsync();
        Task CreateNewTermsOfServiceAsync(string version, string content);
    }
}
