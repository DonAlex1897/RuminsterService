using Microsoft.EntityFrameworkCore;
using RuminsterBackend.Models;
using RuminsterBackend.Services.Interfaces;

namespace RuminsterBackend.Services
{
    public class TermsOfServiceService : ITermsOfServiceService
    {
        private readonly IRequestContextService _contextService;

        public TermsOfServiceService(IRequestContextService contextService)
        {
            _contextService = contextService;
        }

        public async Task<TermsOfService?> GetActiveTermsOfServiceAsync()
        {
            return await _contextService.Context.TermsOfService
                .Where(tos => tos.IsActive)
                .OrderByDescending(tos => tos.CreatedAt)
                .FirstOrDefaultAsync();
        }

        public async Task<bool> HasUserAcceptedLatestTosAsync(string userId)
        {
            var latestTos = await GetActiveTermsOfServiceAsync();
            if (latestTos == null) return true; // If no TOS exists, consider it accepted

            var userAcceptance = await _contextService.Context.UserTosAcceptances
                .Where(uta => uta.UserId == userId && uta.AcceptedVersion == latestTos.Version)
                .FirstOrDefaultAsync();

            return userAcceptance != null;
        }

        public async Task RecordTosAcceptanceAsync(string userId, string tosVersion)
        {
            var tos = await _contextService.Context.TermsOfService
                .Where(t => t.Version == tosVersion && t.IsActive)
                .FirstOrDefaultAsync();

            if (tos == null)
                throw new ArgumentException($"Terms of Service version {tosVersion} not found or not active");

            // Check if user already accepted this version
            var existingAcceptance = await _contextService.Context.UserTosAcceptances
                .Where(uta => uta.UserId == userId && uta.AcceptedVersion == tosVersion)
                .FirstOrDefaultAsync();

            if (existingAcceptance == null)
            {
                var acceptance = new UserTosAcceptance
                {
                    UserId = userId,
                    TermsOfServiceId = tos.Id,
                    AcceptedVersion = tosVersion,
                    AcceptedAt = DateTime.UtcNow
                };

                await _contextService.Context.UserTosAcceptances.AddAsync(acceptance);
                await _contextService.Context.SaveChangesAsync();
            }
        }

        public async Task<List<string>> GetUsersWhoNeedToAcceptLatestTosAsync()
        {
            var latestTos = await GetActiveTermsOfServiceAsync();
            if (latestTos == null) return new List<string>();

            var usersWhoAccepted = await _contextService.Context.UserTosAcceptances
                .Where(uta => uta.AcceptedVersion == latestTos.Version)
                .Select(uta => uta.UserId)
                .ToListAsync();

            var allActiveUsers = await _contextService.Context.Users
                .Where(u => u.EmailConfirmed) // Only include activated users
                .Select(u => u.Id)
                .ToListAsync();

            return allActiveUsers.Except(usersWhoAccepted).ToList();
        }

        public async Task CreateNewTermsOfServiceAsync(string version, string content)
        {
            // Deactivate all existing TOS
            var existingTos = await _contextService.Context.TermsOfService
                .Where(tos => tos.IsActive)
                .ToListAsync();

            foreach (var tos in existingTos)
            {
                tos.IsActive = false;
            }

            // Create new TOS
            var newTos = new TermsOfService
            {
                Version = version,
                Content = content,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            await _contextService.Context.TermsOfService.AddAsync(newTos);
            await _contextService.Context.SaveChangesAsync();
        }
    }
}
