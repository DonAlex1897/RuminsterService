using Microsoft.EntityFrameworkCore;
using RuminsterBackend.Models;

namespace RuminsterBackend.Interceptors.LoggingStrategies
{
    /// <summary>
    /// Logging strategy for UserRelation entities.
    /// Creates UserRelationLog entries when UserRelations are added, updated, or deleted.
    /// 
    /// NOTE: This strategy is currently DISABLED.
    /// To enable:
    /// 1. Uncomment this class
    /// 2. Add "UserRelation" to EnabledEntityTypes in EntityLoggingInterceptor
    /// 3. Register this strategy in EntityLoggingInterceptor constructor
    /// </summary>
    // public class UserRelationLoggingStrategy : IEntityLoggingStrategy
    // {
    //     public string EntityTypeName => "UserRelation";
    //
    //     public void CreateLog(DbContext context, object entity, string operation)
    //     {
    //         var userRelation = (UserRelation)entity;
    //
    //         var log = new UserRelationLog
    //         {
    //             UserRelationId = userRelation.Id,
    //             IsDeleted = userRelation.IsDeleted,
    //             InitiatorId = userRelation.InitiatorId,
    //             ReceiverId = userRelation.ReceiverId,
    //             IsAccepted = userRelation.IsAccepted,
    //             IsRejected = userRelation.IsRejected,
    //             Type = userRelation.Type,
    //             CallerMethod = operation,
    //             CreateById = userRelation.UpdateById,
    //             CreateTMS = userRelation.UpdateTMS,
    //         };
    //
    //         context.Set<UserRelationLog>().Add(log);
    //     }
    //
    //     public async Task CreateLogAsync(DbContext context, object entity, string operation, CancellationToken cancellationToken)
    //     {
    //         var userRelation = (UserRelation)entity;
    //
    //         var log = new UserRelationLog
    //         {
    //             UserRelationId = userRelation.Id,
    //             IsDeleted = userRelation.IsDeleted,
    //             InitiatorId = userRelation.InitiatorId,
    //             ReceiverId = userRelation.ReceiverId,
    //             IsAccepted = userRelation.IsAccepted,
    //             IsRejected = userRelation.IsRejected,
    //             Type = userRelation.Type,
    //             CallerMethod = operation,
    //             CreateById = userRelation.UpdateById,
    //             CreateTMS = userRelation.UpdateTMS,
    //         };
    //
    //         await context.Set<UserRelationLog>().AddAsync(log, cancellationToken);
    //     }
    // }
}
