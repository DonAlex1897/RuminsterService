using Microsoft.EntityFrameworkCore;
using RuminsterBackend.Models;

namespace RuminsterBackend.Interceptors.LoggingStrategies
{
    /// <summary>
    /// Logging strategy for Rumination entities.
    /// Creates RuminationLog entries when Ruminations are added, updated, or deleted.
    /// </summary>
    public class RuminationLoggingStrategy : IEntityLoggingStrategy
    {
        public string EntityTypeName => "Rumination";

        public void CreateLog(DbContext context, object entity, string operation)
        {
            var rumination = (Rumination)entity;

            var log = new RuminationLog
            {
                RuminationId = rumination.Id, // ID is now populated after save
                IsDeleted = rumination.IsDeleted,
                Content = rumination.Content,
                IsPublished = rumination.IsPublished,
                CallerMethod = operation,
                CreateById = rumination.UpdateById,
                CreateTMS = rumination.UpdateTMS,
            };

            context.Set<RuminationLog>().Add(log);
        }

        public async Task CreateLogAsync(DbContext context, object entity, string operation, CancellationToken cancellationToken)
        {
            var rumination = (Rumination)entity;

            var log = new RuminationLog
            {
                RuminationId = rumination.Id, // ID is now populated after save
                IsDeleted = rumination.IsDeleted,
                Content = rumination.Content,
                IsPublished = rumination.IsPublished,
                CallerMethod = operation,
                CreateById = rumination.UpdateById,
                CreateTMS = rumination.UpdateTMS,
            };

            await context.Set<RuminationLog>().AddAsync(log, cancellationToken);
        }
    }
}
