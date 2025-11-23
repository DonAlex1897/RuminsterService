using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RuminsterBackend.Data;
using RuminsterBackend.Exceptions;
using RuminsterBackend.Models;
using RuminsterBackend.Models.DTOs.Rumination;
using RuminsterBackend.Models.DTOs.User;
using RuminsterBackend.Models.Enums;
using RuminsterBackend.Services.Interfaces;

namespace RuminsterBackend.Services
{
    public class RuminationsService(
        IRequestContextService requestContextService,
        ITextSearchService textSearchService
    ) : IRuminationsService
    {
        private readonly IRequestContextService _requestContextService = requestContextService;

        private readonly ITextSearchService _textSearchService = textSearchService;

        private readonly DateTime _currentTime = requestContextService.Time;

        private readonly RuminsterDbContext _context = requestContextService.Context;

        public async Task<List<RuminationResponse>> GetRuminationsAsync(GetRuminationsQueryParams queryParams)
        {
            var ruminationsQuery = _context.Ruminations
                .Include(q => q.Logs)
                .Include(q => q.Audiences)
                .Include(q => q.CreateBy)
                .Include(q => q.UpdateBy)
                .Where(q => q.IsPublished)
                .AsNoTracking()
                .AsQueryable();

            if (queryParams.Id?.Count > 0)
            {
                ruminationsQuery = ruminationsQuery
                    .Where(q => queryParams.Id.Contains(q.Id));
            }

            if (queryParams.Content?.Count > 0)
            {
                ruminationsQuery = ruminationsQuery
                    .Where(q => queryParams.Content.Any(p => EF.Functions
                        .Like(q.Content, p)));
            }

            if (queryParams.UserId?.Count > 0)
            {
                ruminationsQuery = ruminationsQuery
                    .Where(q => queryParams.UserId.Contains(q.CreateById));
            }

            if (queryParams.FromTMS.HasValue)
            {
                ruminationsQuery = ruminationsQuery
                    .Where(q => q.UpdateTMS > queryParams.FromTMS.Value);
            }

            if (queryParams.ToTMS.HasValue)
            {
                ruminationsQuery = ruminationsQuery
                    .Where(q => q.UpdateTMS < queryParams.ToTMS.Value);
            }
            
            if (!queryParams.IncludeDeleted.HasValue || queryParams.IncludeDeleted.Value == false)
            {
                ruminationsQuery = ruminationsQuery
                    .Where(q => !q.IsDeleted);
            }

            if (queryParams.IsPublic)
            {
                ruminationsQuery = ruminationsQuery
                    .Where(q => q.Audiences == null || !q.Audiences.Any(q => !q.IsDeleted));
            }
            else
            {
                var relatedUserIdsQuery = _context.UserRelations
                    .Where(q => (q.ReceiverId == _requestContextService.User.Id || q.InitiatorId == _requestContextService.User.Id) &&
                                q.IsAccepted && !q.IsDeleted)
                    .Select(q => new
                    {
                        UserId = q.InitiatorId != _requestContextService.User.Id ? q.InitiatorId : q.ReceiverId,
                        RelationType = q.Type
                    });

                ruminationsQuery = ruminationsQuery.Where(r =>
                    r.Audiences.Any(a =>
                        !a.IsDeleted &&
                        relatedUserIdsQuery.Any(ru => ru.UserId == r.CreateById && ru.RelationType == a.RelationType)
                    ));
            }

            // Sort
            if (!string.IsNullOrEmpty(queryParams.Sort))
            {
                try
                {
                    ruminationsQuery = ruminationsQuery.OrderBy(queryParams.Sort);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Error: Invalid sort parameter.\n" + e);
                }
            }
            else
            {
                ruminationsQuery = ruminationsQuery
                    .OrderByDescending(q => q.UpdateTMS);  // Default sort
            }

            if (queryParams.Offset != null)
            {
                ruminationsQuery = ruminationsQuery
                    .Skip(queryParams.Offset.Value);
            }

            if (queryParams.Limit != null)
            {
                ruminationsQuery = ruminationsQuery
                    .Take(queryParams.Limit.Value);
            }

            var ruminations = await ruminationsQuery.ToListAsync();

            var ruminationsResponse = ruminations
                .Select(RuminationMapper.MapRuminationResponse)
                .ToList();
            
            return ruminationsResponse;
        }

        public async Task<List<RuminationResponse>> GetMyRuminationsAsync(GetMyRuminationsQueryParams queryParams)
        {
            var ruminationsQuery = _context.Ruminations
                .Include(q => q.Logs)
                .Include(q => q.Audiences)
                .Include(q => q.CreateBy)
                .Include(q => q.UpdateBy)
                .Where(q => q.CreateById == _requestContextService.User.Id)
                .Where(q => q.IsPublished == queryParams.IsPublic)
                .AsNoTracking()
                .AsQueryable();

            if (queryParams.Content?.Count > 0)
            {
                ruminationsQuery = ruminationsQuery
                    .Where(q => queryParams.Content.Any(p => EF.Functions
                        .Like(q.Content, p)));
            }

            if (queryParams.FromTMS.HasValue)
            {
                ruminationsQuery = ruminationsQuery
                    .Where(q => q.UpdateTMS > queryParams.FromTMS.Value);
            }

            if (queryParams.ToTMS.HasValue)
            {
                ruminationsQuery = ruminationsQuery
                    .Where(q => q.UpdateTMS < queryParams.ToTMS.Value);
            }
            
            if (!queryParams.IncludeDeleted.HasValue || queryParams.IncludeDeleted.Value == false)
            {
                ruminationsQuery = ruminationsQuery
                    .Where(q => !q.IsDeleted);
            }

            // Sort
            if (!string.IsNullOrEmpty(queryParams.Sort))
            {
                try
                {
                    ruminationsQuery = ruminationsQuery.OrderBy(queryParams.Sort);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Error: Invalid sort parameter.\n" + e);
                }
            }
            else
            {
                ruminationsQuery = ruminationsQuery
                    .OrderByDescending(q => q.UpdateTMS);  // Default sort
            }

            if (queryParams.Offset != null)
            {
                ruminationsQuery = ruminationsQuery
                    .Skip(queryParams.Offset.Value);
            }

            if (queryParams.Limit != null)
            {
                ruminationsQuery = ruminationsQuery
                    .Take(queryParams.Limit.Value);
            }

            var ruminations = await ruminationsQuery.ToListAsync();

            var ruminationsResponse = ruminations
                .Select(RuminationMapper.MapRuminationResponse)
                .ToList();
            
            return ruminationsResponse;
        }

        public async Task<RuminationResponse> GetRuminationAsync(int ruminationId)
        {
            var rumination = await _context.Ruminations
                .Include(q => q.Logs)
                .Include(q => q.Audiences)
                .Include(q => q.CreateBy)
                .Include(q => q.UpdateBy)
                .Where(q => !q.IsDeleted)
                .Where(q => q.Id == ruminationId)
                .FirstOrDefaultAsync() ?? 
                throw new NotFoundException($"{ruminationId} is not a valid Rumination Id");
            
            var ruminationResponse = RuminationMapper.MapRuminationResponse(rumination);
            return ruminationResponse;
        }

        public async Task<RuminationResponse> PostRuminationAsync(PostRuminationDto dto)
        {
            List<RuminationAudience> audiences = [..dto.Audiences?.Select(q => new RuminationAudience
            {
                RelationType = q,
                CreateTMS = _currentTime,
                UpdateTMS = _currentTime,
            }) ?? []];

            var newRumination = new Rumination
            {
                Content = dto.Content,
                IsPublished = dto.Publish,
                Audiences = audiences,
                Logs = [],
                CreateById = _requestContextService.User.Id,
                CreateTMS = _currentTime,
                UpdateById = _requestContextService.User.Id,
                UpdateTMS = _currentTime,
            };

            await _context.Ruminations.AddAsync(newRumination);
            await _context.SaveChangesAsync();

            var ruminationResponse = RuminationMapper.MapRuminationResponse(newRumination);
            return ruminationResponse;
        }

        public async Task<RuminationResponse> PutRuminationContentAsync(int ruminationId, PutRuminationContentDto dto)
        {
            var rumination = await _context.Ruminations
                .Include(q => q.Logs)
                .Include(q => q.Audiences)
                .Include(q => q.CreateBy)
                .Include(q => q.UpdateBy)
                .Where(q => !q.IsDeleted)
                .Where(q => q.Id == ruminationId)
                .FirstOrDefaultAsync() ?? 
                throw new NotFoundException($"{ruminationId} is not a valid Rumination Id");
            
            if (rumination.CreateById != _requestContextService.User.Id)
            {
                throw new ForbiddenException("You do not have permission to edit this rumination.");
            }
            
            rumination.Content = dto.Content;
            rumination.UpdateById = _requestContextService.User.Id;
            rumination.UpdateTMS = _currentTime;
            _context.Ruminations.Update(rumination);
            await _context.SaveChangesAsync();

            var ruminationResponse = RuminationMapper.MapRuminationResponse(rumination); 
            return ruminationResponse;
        }

        public async Task<RuminationResponse> PutRuminationVisibilityAsync(int ruminationId)
        {
            var rumination = await _context.Ruminations
                .Include(q => q.Logs)
                .Include(q => q.Audiences)
                .Include(q => q.CreateBy)
                .Include(q => q.UpdateBy)
                .Where(q => !q.IsDeleted)
                .Where(q => q.Id == ruminationId)
                .FirstOrDefaultAsync() ?? 
                throw new NotFoundException($"{ruminationId} is not a valid Rumination Id");
            
            if (rumination.CreateById != _requestContextService.User.Id)
            {
                throw new ForbiddenException("You do not have permission to edit this rumination.");
            }
            
            rumination.IsPublished = !rumination.IsPublished;
            rumination.UpdateById = _requestContextService.User.Id;
            rumination.UpdateTMS = _currentTime;
            _context.Ruminations.Update(rumination);
            await _context.SaveChangesAsync();

            var ruminationResponse = RuminationMapper.MapRuminationResponse(rumination); 
            return ruminationResponse;
        }

        public async Task DeleteRuminationAsync(int ruminationId)
        {
            var rumination = await _context.Ruminations
                .Include(q => q.Logs)
                .Include(q => q.CreateBy)
                .Include(q => q.UpdateBy)
                .Where(q => !q.IsDeleted)
                .Where(q => q.Id == ruminationId)
                .FirstOrDefaultAsync() ?? 
                throw new NotFoundException($"{ruminationId} is not a valid Rumination Id");
            
            if (rumination.CreateById != _requestContextService.User.Id)
            {
                throw new ForbiddenException("You do not have permission to delete this rumination.");
            }
            
            rumination.IsDeleted = true;
            rumination.UpdateById = _requestContextService.User.Id;
            rumination.UpdateTMS = _currentTime;
            _context.Ruminations.Update(rumination);
            await _context.SaveChangesAsync();
        }

        public async Task<RuminationResponse> PutRuminationAudiencesAsync(int ruminationId, PutRuminationAudiencesDto dto)
        {
            var rumination = await _context.Ruminations
                .Include(q => q.Logs)
                .Include(q => q.Audiences)
                .Include(q => q.CreateBy)
                .Include(q => q.UpdateBy)
                .Where(q => !q.IsDeleted)
                .Where(q => q.Id == ruminationId)
                .FirstOrDefaultAsync() ?? 
                throw new NotFoundException($"{ruminationId} is not a valid Rumination Id");
            
            if (rumination.CreateById != _requestContextService.User.Id)
            {
                throw new ForbiddenException("You do not have permission to edit this rumination.");
            }
            
            List<RuminationAudience> currentAudiences = [..rumination.Audiences?.Where(q => !q.IsDeleted) ?? []];

            currentAudiences.ForEach(q => {
                if (!(dto.Audiences?.Contains(q.RelationType) ?? false))
                {
                    q.IsDeleted = true;
                }
            });

            _context.RuminationAudiences.UpdateRange(currentAudiences);
            
            List<RuminationAudience> newAudiences = [..dto.Audiences?
                .Where(q => !currentAudiences.Select(a => a.RelationType).ToList().Contains(q))?
                .Select(q => new RuminationAudience
                {
                    RuminationId = rumination.Id,
                    RelationType = q,
                    CreateTMS = _currentTime,
                    UpdateTMS = _currentTime,
                }) ?? []];

            await _context.RuminationAudiences.AddRangeAsync(newAudiences);

            rumination.UpdateById = _requestContextService.User.Id;
            rumination.UpdateTMS = _currentTime;
            _context.Ruminations.Update(rumination);
            await _context.SaveChangesAsync();
            
            var ruminationResponse = RuminationMapper.MapRuminationResponse(rumination); 
            return ruminationResponse;
        }

        public async Task<RuminationResponse> PutRuminationAsync(int ruminationId, PutRuminationDto dto)
        {
            var rumination = await _context.Ruminations
                .Include(q => q.Logs)
                .Include(q => q.Audiences)
                .Include(q => q.CreateBy)
                .Include(q => q.UpdateBy)
                .Where(q => !q.IsDeleted)
                .Where(q => q.Id == ruminationId)
                .FirstOrDefaultAsync() ?? 
                throw new NotFoundException($"{ruminationId} is not a valid Rumination Id");
            
            if (rumination.CreateById != _requestContextService.User.Id)
            {
                throw new ForbiddenException("You do not have permission to edit this rumination.");
            }
            
            // Update content
            rumination.Content = dto.Content;
            rumination.IsPublished = dto.Publish;
            
            // Update audiences
            List<RuminationAudience> currentAudiences = [..rumination.Audiences?.Where(q => !q.IsDeleted) ?? []];

            currentAudiences.ForEach(q => {
                if (!(dto.Audiences?.Contains(q.RelationType) ?? false))
                {
                    q.IsDeleted = true;
                }
            });

            _context.RuminationAudiences.UpdateRange(currentAudiences);
            
            List<RuminationAudience> newAudiences = [..dto.Audiences?
                .Where(q => !currentAudiences.Select(a => a.RelationType).ToList().Contains(q))?
                .Select(q => new RuminationAudience
                {
                    RuminationId = rumination.Id,
                    RelationType = q,
                    CreateTMS = _currentTime,
                    UpdateTMS = _currentTime,
                }) ?? []];

            await _context.RuminationAudiences.AddRangeAsync(newAudiences);

            rumination.UpdateById = _requestContextService.User.Id;
            rumination.UpdateTMS = _currentTime;
            _context.Ruminations.Update(rumination);
            await _context.SaveChangesAsync();
            
            var ruminationResponse = RuminationMapper.MapRuminationResponse(rumination); 
            return ruminationResponse;
        }

        public async Task<List<RuminationResponse>> SearchAccessibleRuminationsAsync(string query, int? limit = 10, int? offset = null)
        {
            var q = _textSearchService.Normalize(query);
            if (string.IsNullOrWhiteSpace(q))
            {
                return new List<RuminationResponse>();
            }

            var ruminationsQuery = _requestContextService.Context.Ruminations
                .Include(q => q.Logs)
                .Include(q => q.Audiences)
                .Include(q => q.CreateBy)
                .Include(q => q.UpdateBy)
                .Where(r => r.IsPublished)
                .Where(r => !r.IsDeleted)
                .AsNoTracking()
                .AsSplitQuery()
                .AsQueryable();

            // Access gating for feed-like visibility (same as GetRuminationsAsync with IsPublic=false)
            var relatedUserIdsQuery = _requestContextService.Context.UserRelations
                .Where(ur => (ur.ReceiverId == _requestContextService.User.Id || ur.InitiatorId == _requestContextService.User.Id)
                             && ur.IsAccepted && !ur.IsDeleted)
                .Select(ur => new
                {
                    UserId = ur.InitiatorId != _requestContextService.User.Id ? ur.InitiatorId : ur.ReceiverId,
                    RelationType = ur.Type
                });

            ruminationsQuery = ruminationsQuery.Where(r =>
                // Public ruminations: no non-deleted audiences
                !r.Audiences.Any(a => !a.IsDeleted)
                ||
                // Feed-visible ruminations: at least one audience matching a relation
                r.Audiences.Any(a => !a.IsDeleted &&
                    relatedUserIdsQuery.Any(ru => ru.UserId == r.CreateById && ru.RelationType == a.RelationType))
            );

            // Text search on content
            ruminationsQuery = _textSearchService.ApplyContainsFilter(ruminationsQuery, q, r => r.Content);

            // Sort fresh first
            ruminationsQuery = ruminationsQuery.OrderByDescending(r => r.UpdateTMS);

            // Pagination
            ruminationsQuery = _textSearchService.ApplyPagination(ruminationsQuery, offset, limit, 50);

            var ruminations = await ruminationsQuery.ToListAsync();
            return ruminations.Select(RuminationMapper.MapRuminationResponse).ToList();
        }
    }
}