using Microsoft.EntityFrameworkCore;

namespace RuminsterBackend.Interceptors.LoggingStrategies
{
    /// <summary>
    /// Interface for entity-specific logging strategies.
    /// Implement this interface to add automatic logging for a new entity type.
    /// </summary>
    public interface IEntityLoggingStrategy
    {
        /// <summary>
        /// The name of the entity type this strategy handles (e.g., "Rumination", "Comment").
        /// </summary>
        string EntityTypeName { get; }

        /// <summary>
        /// Creates a log entry for the entity synchronously.
        /// </summary>
        /// <param name="context">The database context.</param>
        /// <param name="entity">The entity to log.</param>
        /// <param name="operation">The operation performed (Add, Update, Delete).</param>
        void CreateLog(DbContext context, object entity, string operation);

        /// <summary>
        /// Creates a log entry for the entity asynchronously.
        /// </summary>
        /// <param name="context">The database context.</param>
        /// <param name="entity">The entity to log.</param>
        /// <param name="operation">The operation performed (Add, Update, Delete).</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        Task CreateLogAsync(DbContext context, object entity, string operation, CancellationToken cancellationToken);
    }
}
