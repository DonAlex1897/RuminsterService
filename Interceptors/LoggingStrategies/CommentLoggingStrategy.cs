using Microsoft.EntityFrameworkCore;
using RuminsterBackend.Models;

namespace RuminsterBackend.Interceptors.LoggingStrategies
{
    /// <summary>
    /// Logging strategy for Comment entities.
    /// Creates CommentLog entries when Comments are added, updated, or deleted.
    /// 
    /// NOTE: This strategy is currently DISABLED.
    /// To enable:
    /// 1. Uncomment this class
    /// 2. Create the CommentLog model
    /// 3. Add "Comment" to EnabledEntityTypes in EntityLoggingInterceptor
    /// 4. Register this strategy in EntityLoggingInterceptor constructor
    /// </summary>
    // public class CommentLoggingStrategy : IEntityLoggingStrategy
    // {
    //     public string EntityTypeName => "Comment";
    //
    //     public void CreateLog(DbContext context, object entity, string operation)
    //     {
    //         var comment = (Comment)entity;
    //
    //         var log = new CommentLog
    //         {
    //             CommentId = comment.Id,
    //             IsDeleted = comment.IsDeleted,
    //             Content = comment.Content,
    //             CallerMethod = operation,
    //             CreateById = comment.UpdateById,
    //             CreateTMS = comment.UpdateTMS,
    //         };
    //
    //         context.Set<CommentLog>().Add(log);
    //     }
    //
    //     public async Task CreateLogAsync(DbContext context, object entity, string operation, CancellationToken cancellationToken)
    //     {
    //         var comment = (Comment)entity;
    //
    //         var log = new CommentLog
    //         {
    //             CommentId = comment.Id,
    //             IsDeleted = comment.IsDeleted,
    //             Content = comment.Content,
    //             CallerMethod = operation,
    //             CreateById = comment.UpdateById,
    //             CreateTMS = comment.UpdateTMS,
    //         };
    //
    //         await context.Set<CommentLog>().AddAsync(log, cancellationToken);
    //     }
    // }
}
