using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.Linq.Dynamic.Core;
using RuminsterBackend.Data;
using RuminsterBackend.Exceptions;
using RuminsterBackend.Models;
using RuminsterBackend.Models.DTOs.User;
using RuminsterBackend.Models.DTOs.UserRelation;
using RuminsterBackend.Services.Interfaces;

namespace RuminsterBackend.Services
{
    public class UserRelationsService(IRequestContextService requestContextService) : IUserRelationsService
    {        
        private readonly UserResponse _user = requestContextService.User;

        private readonly DateTime _currentTime = requestContextService.Time;

        private readonly RuminsterDbContext _context = requestContextService.Context;

        public static UserRelationLog MapToLog(UserRelation userRelation, string callerMethod)
        {
            return new UserRelationLog
            {
                UserRelationId = userRelation.Id,
                IsDeleted = userRelation.IsDeleted,
                InitiatorId = userRelation.InitiatorId,
                ReceiverId = userRelation.ReceiverId,
                IsAccepted = userRelation.IsAccepted,
                IsRejected = userRelation.IsRejected,
                Type = userRelation.Type,
                CreateById = userRelation.UpdateById,
                CreateTMS = userRelation.UpdateTMS,
            };
        }

        public async Task<List<UserRelationResponse>> GetUserRelationsAsync(GetUserRelationsQueryParams queryParams)
        {
            var userRelationsQuery = _context.UserRelations
                .Include(q => q.Initiator)
                .Include(q => q.Receiver)
                .AsNoTracking()
                .AsQueryable();
            
            if (queryParams.Id != null && queryParams.Id.Count > 0)
            {
                userRelationsQuery = userRelationsQuery.Where(q => queryParams.Id.Contains(q.Id));
            }

            if (queryParams.UserId != null)
            {
                if (queryParams.WithMe.HasValue && queryParams.WithMe.Value)
                {
                    userRelationsQuery = userRelationsQuery
                        .Where(q =>
                            (q.InitiatorId == queryParams.UserId && q.ReceiverId == _user.Id) ||
                            (q.ReceiverId == queryParams.UserId && q.InitiatorId == _user.Id));
                }
                else
                {
                    userRelationsQuery = userRelationsQuery.Where(q => q.InitiatorId == queryParams.UserId || q.ReceiverId == queryParams.UserId);
                }
            }

            if (queryParams.FromTMS.HasValue)
            {
                userRelationsQuery = userRelationsQuery.Where(q => q.CreateTMS >= queryParams.FromTMS.Value);
            }

            if (queryParams.ToTMS.HasValue)
            {
                userRelationsQuery = userRelationsQuery.Where(q => q.CreateTMS <= queryParams.ToTMS.Value);
            }

            if (!queryParams.IncludeDeleted.HasValue || !queryParams.IncludeDeleted.Value)
            {
                userRelationsQuery = userRelationsQuery.Where(q => !q.IsDeleted);
            }

            // Sort
            if (!string.IsNullOrEmpty(queryParams.Sort))
            {
                try
                {
                    userRelationsQuery = userRelationsQuery.OrderBy(queryParams.Sort);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Error: Invalid sort parameter.\n" + e);
                }
            }
            else
            {
                userRelationsQuery = userRelationsQuery
                    .OrderByDescending(q => q.UpdateTMS);  // Default sort
            }

            if (queryParams.Offset != null)
            {
                userRelationsQuery = userRelationsQuery
                    .Skip(queryParams.Offset.Value);
            }

            if (queryParams.Limit != null)
            {
                userRelationsQuery = userRelationsQuery
                    .Take(queryParams.Limit.Value);
            }

            var userRelations = await userRelationsQuery.ToListAsync();

            return [.. userRelations.Select(UserRelationMapper.MapUserRelationResponse)];
        }

        public async Task<UserRelationResponse> GetUserRelationAsync(int userRelationId)
        {
            var userRelation = await _context.UserRelations
                .Include(q => q.Initiator)
                .Include(q => q.Receiver)
                .Where(q => !q.IsDeleted)
                .Where(q => q.Id == userRelationId)
                .FirstOrDefaultAsync() ?? 
                throw new NotFoundException($"{userRelationId} is not a valid user relation id.");

            return UserRelationMapper.MapUserRelationResponse(userRelation);
        }

        public async Task<UserRelationResponse> PostUserRelationAsync(PostUserRelationDto dto)
        {
            var userRelation = await _context.UserRelations
                .Where(q => !q.IsDeleted)
                .Where(q => !q.IsRejected)
                .Where(q =>
                    (q.InitiatorId == _user.Id && q.ReceiverId == dto.UserId) ||
                    (q.InitiatorId == dto.UserId && q.ReceiverId == _user.Id))
                .Where(q => q.Type == dto.RelationType)
                .FirstOrDefaultAsync();
            
            if (userRelation != default)
            {
                throw new ForbiddenException("This relation already exists.");
            }

            if (dto.UserId == _user.Id)
            {
                throw new ForbiddenException("The target user must not be the current user.");
            }

            var newUserRelation = new UserRelation
            {
                InitiatorId = _user.Id,
                ReceiverId = dto.UserId,
                Type = dto.RelationType,
                Logs = [],
                CreateById = _user.Id,
                CreateTMS = _currentTime,
                UpdateById = _user.Id,
                UpdateTMS = _currentTime,
            };

            newUserRelation.Logs.Add(MapToLog(newUserRelation, "PostUserRelationResponseAsync"));
            await _context.UserRelations.AddAsync(newUserRelation);
            await _context.SaveChangesAsync();

            return await GetUserRelationAsync(newUserRelation.Id);
        }

        public async Task<UserRelationResponse> PutUserRelationAcceptAsync(int userRelationId)
        {
            var userRelation = await _context.UserRelations
                .Include(q => q.Logs)
                .Where(q => !q.IsDeleted)
                .Where(q => q.Id == userRelationId)
                .FirstOrDefaultAsync() ?? 
                throw new NotFoundException($"{userRelationId} is not a valid user relation id.");

            if (userRelation.ReceiverId != _user.Id)
            {
                throw new ForbiddenException("You do not have permission to accept this relation.");
            }

            if (userRelation.IsRejected)
            {
                throw new ForbiddenException("Cannot accept a rejected request. You should make a new request.");
            }

            userRelation.IsAccepted = true;
            userRelation.UpdateById = _user.Id;
            userRelation.UpdateTMS = _currentTime;
            userRelation.Logs.Add(MapToLog(userRelation, "PutUserRelationAcceptAsync"));
            _context.UserRelations.Update(userRelation);
            await _context.SaveChangesAsync();

            return await GetUserRelationAsync(userRelation.Id);
        }

        public async Task<UserRelationResponse> PutUserRelationRejectAsync(int userRelationId)
        {
            var userRelation = await _context.UserRelations
                .Include(q => q.Logs)
                .Where(q => !q.IsDeleted)
                .Where(q => q.Id == userRelationId)
                .FirstOrDefaultAsync() ?? 
                throw new NotFoundException($"{userRelationId} is not a valid user relation id.");

            if (userRelation.ReceiverId != _user.Id)
            {
                throw new ForbiddenException("You do not have permission to reject this relation.");
            }

            if (userRelation.IsAccepted)
            {
                throw new ForbiddenException("Cannot reject an accepted request. You need to request to delete the relation.");
            }

            userRelation.IsRejected = true;
            userRelation.UpdateById = _user.Id;
            userRelation.UpdateTMS = _currentTime;
            userRelation.Logs.Add(MapToLog(userRelation, "PutUserRelationRejectAsync"));
            _context.UserRelations.Update(userRelation);
            await _context.SaveChangesAsync();

            return await GetUserRelationAsync(userRelation.Id);
        }

        public async Task DeleteUserRelationAsync(int userRelationId)
        {
            var userRelation = await _context.UserRelations
                .Include(q => q.Logs)
                .Where(q => !q.IsDeleted)
                .Where(q => q.Id == userRelationId)
                .FirstOrDefaultAsync() ?? 
                throw new NotFoundException($"{userRelationId} is not a valid user relation id.");

            if (userRelation.ReceiverId != _user.Id && userRelation.InitiatorId != _user.Id)
            {
                throw new ForbiddenException("You do not have permission to delete this relation.");
            }

            userRelation.IsDeleted = true;
            userRelation.UpdateById = _user.Id;
            userRelation.UpdateTMS = _currentTime;
            userRelation.Logs.Add(MapToLog(userRelation, "DeleteUserRelationAsync"));
            _context.UserRelations.Update(userRelation);
            await _context.SaveChangesAsync();
        }
    }
}