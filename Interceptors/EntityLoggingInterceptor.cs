using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using RuminsterBackend.Interceptors.LoggingStrategies;

namespace RuminsterBackend.Interceptors
{
    /// <summary>
    /// Automatically creates audit logs for entities when they are added, updated, or deleted.
    /// This interceptor uses a two-phase approach:
    /// 1. SavingChanges: Captures which entities are being modified (before IDs are assigned)
    /// 2. SavedChanges: Creates logs after the main save completes (when IDs are populated)
    /// 
    /// Currently enabled for:
    /// - Rumination → RuminationLog
    /// 
    /// To enable logging for other entities:
    /// 1. Add the entity name to EnabledEntityTypes (line ~38)
    /// 2. Uncomment the strategy class in LoggingStrategies/ folder
    /// 3. Register the strategy in the constructor (line ~46)
    /// </summary>
    public class EntityLoggingInterceptor : SaveChangesInterceptor
    {
        /// <summary>
        /// Dictionary of registered logging strategies, keyed by entity type name.
        /// </summary>
        private readonly Dictionary<string, IEntityLoggingStrategy> _strategies = new();

        /// <summary>
        /// List of entity types that should have automatic logging enabled.
        /// Add entity names here to enable logging for them.
        /// </summary>
        private static readonly HashSet<string> EnabledEntityTypes = new()
        {
            "Rumination",
            // "Comment",      // Uncomment when CommentLoggingStrategy is ready
            // "UserRelation", // Uncomment when UserRelationLoggingStrategy is ready
        };

        public EntityLoggingInterceptor()
        {
            // Register active logging strategies here
            RegisterStrategy(new RuminationLoggingStrategy());
            
            // Uncomment to enable additional strategies:
            // RegisterStrategy(new CommentLoggingStrategy());
            // RegisterStrategy(new UserRelationLoggingStrategy());
        }

        private void RegisterStrategy(IEntityLoggingStrategy strategy)
        {
            _strategies[strategy.EntityTypeName] = strategy;
        }

        [ThreadStatic]
        private static List<(object Entity, string Operation, string EntityType)>? _pendingLogs;

        private static List<(object Entity, string Operation, string EntityType)> PendingLogs
        {
            get
            {
                _pendingLogs ??= new List<(object Entity, string Operation, string EntityType)>();
                return _pendingLogs;
            }
        }

        public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
        {
            CaptureEntityChanges(eventData);
            return base.SavingChanges(eventData, result);
        }

        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData, 
            InterceptionResult<int> result, 
            CancellationToken cancellationToken = default)
        {
            CaptureEntityChanges(eventData);
            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }

        public override int SavedChanges(SaveChangesCompletedEventData eventData, int result)
        {
            CreateEntityLogs(eventData);
            return base.SavedChanges(eventData, result);
        }

        public override async ValueTask<int> SavedChangesAsync(
            SaveChangesCompletedEventData eventData,
            int result,
            CancellationToken cancellationToken = default)
        {
            await CreateEntityLogsAsync(eventData, cancellationToken);
            return await base.SavedChangesAsync(eventData, result, cancellationToken);
        }

        /// <summary>
        /// Captures entity changes before they are saved to the database.
        /// Automatically processes all entities listed in EnabledEntityTypes.
        /// </summary>
        private void CaptureEntityChanges(DbContextEventData eventData)
        {
            if (eventData.Context == null)
                return;

            PendingLogs.Clear();
            var context = eventData.Context;

            var entries = context.ChangeTracker.Entries()
                .Where(e => e.State == EntityState.Added || 
                           e.State == EntityState.Modified || 
                           e.State == EntityState.Deleted)
                .ToList();

            foreach (var entry in entries)
            {
                var entityType = entry.Entity.GetType().Name;
                
                // Only process entities that are enabled for logging
                if (!EnabledEntityTypes.Contains(entityType))
                    continue;
                
                var operation = GetOperationType(entry.State, entityType);
                PendingLogs.Add((entry.Entity, operation, entityType));
            }
        }

        /// <summary>
        /// Creates log entries after entities have been saved.
        /// Uses registered strategies to create entity-specific logs.
        /// </summary>
        private void CreateEntityLogs(SaveChangesCompletedEventData eventData)
        {
            if (eventData.Context == null || !PendingLogs.Any())
                return;

            var context = eventData.Context;

            foreach (var (entity, operation, entityType) in PendingLogs)
            {
                if (_strategies.TryGetValue(entityType, out var strategy))
                {
                    strategy.CreateLog(context, entity, operation);
                }
            }

            PendingLogs.Clear();

            // Save the logs
            context.SaveChanges();
        }

        /// <summary>
        /// Async version of CreateEntityLogs.
        /// Uses registered strategies to create entity-specific logs.
        /// </summary>
        private async Task CreateEntityLogsAsync(SaveChangesCompletedEventData eventData, CancellationToken cancellationToken)
        {
            if (eventData.Context == null || !PendingLogs.Any())
                return;

            var context = eventData.Context;

            foreach (var (entity, operation, entityType) in PendingLogs)
            {
                if (_strategies.TryGetValue(entityType, out var strategy))
                {
                    await strategy.CreateLogAsync(context, entity, operation, cancellationToken);
                }
            }

            PendingLogs.Clear();

            // Save the logs
            await context.SaveChangesAsync(cancellationToken);
        }

        private string GetOperationType(EntityState state, string entityType)
        {
            return state switch
            {
                EntityState.Added => $"{entityType}.Add",
                EntityState.Modified => $"{entityType}.Update",
                // Note: EntityState.Deleted is not handled because:
                // 1. We use soft deletes (IsDeleted = true), which appears as Modified
                // 2. Hard deletes would remove the entity, making the foreign key invalid
                _ => $"{entityType}.Unknown"
            };
        }
    }
}
