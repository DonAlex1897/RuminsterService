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
        IRequestContextService requestContextService
    ) : IRuminationsService
    {
        private readonly UserResponse _user = requestContextService.User;

        private readonly DateTime _currentTime = requestContextService.Time;

        private readonly RuminsterDbContext _context = requestContextService.Context;

        public static RuminationLog MapToLog(Rumination rumination, string callerMethod)
        {
            return new RuminationLog
            {
                RuminationId = rumination.Id,
                IsDeleted = rumination.IsDeleted,
                Content = rumination.Content,
                IsPublic = rumination.IsPublished,
                CallerMethod = callerMethod,
                CreateById = rumination.UpdateById,
                CreateTMS = rumination.UpdateTMS,
            };
        }

        public async Task<List<RuminationResponse>> GetPublicRuminationsAsync(GetRuminationsQueryParams queryParams)
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

            ruminationsQuery = ruminationsQuery
                .Where(q => q.Audiences == null || !q.Audiences.Any(q => !q.IsDeleted));

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
                .Where(q => q.CreateById == _user.Id)
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
                IsPublished = dto.IsPublic,
                Audiences = audiences,
                Logs = [],
                CreateById = _user.Id,
                CreateTMS = _currentTime,
                UpdateById = _user.Id,
                UpdateTMS = _currentTime,
            };

            newRumination.Logs.Add(MapToLog(newRumination, "PostRuminationAsync"));
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
            
            if (rumination.CreateById != _user.Id)
            {
                throw new ForbiddenException("You do not have permission to edit this rumination.");
            }
            
            rumination.Content = dto.Content;
            rumination.UpdateById = _user.Id;
            rumination.UpdateTMS = _currentTime;
            rumination.Logs.Add(MapToLog(rumination, "PutRuminationContentAsync"));
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
            
            if (rumination.CreateById != _user.Id)
            {
                throw new ForbiddenException("You do not have permission to edit this rumination.");
            }
            
            rumination.IsPublished = !rumination.IsPublished;
            rumination.UpdateById = _user.Id;
            rumination.UpdateTMS = _currentTime;
            rumination.Logs.Add(MapToLog(rumination, "PutRuminationVisibilityAsync"));
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
            
            if (rumination.CreateById != _user.Id)
            {
                throw new ForbiddenException("You do not have permission to delete this rumination.");
            }
            
            rumination.IsDeleted = true;
            rumination.UpdateById = _user.Id;
            rumination.UpdateTMS = _currentTime;
            rumination.Logs.Add(MapToLog(rumination, "DeleteRuminationAsync"));
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
            
            if (rumination.CreateById != _user.Id)
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

            rumination.UpdateById = _user.Id;
            rumination.UpdateTMS = _currentTime;
            rumination.Logs.Add(MapToLog(rumination, "PutRuminationAudiencesAsync"));
            _context.Ruminations.Update(rumination);
            await _context.SaveChangesAsync();
            
            var ruminationResponse = RuminationMapper.MapRuminationResponse(rumination); 
            return ruminationResponse;
        }
    }
}