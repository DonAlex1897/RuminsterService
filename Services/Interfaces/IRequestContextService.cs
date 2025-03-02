using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RuminsterBackend.Data;
using RuminsterBackend.Models.DTOs.User;

namespace RuminsterBackend.Services.Interfaces
{
    public interface IRequestContextService
    {
        UserResponse User { get; }

        DateTime Time { get; }

        RuminsterDbContext Context { get; set; }
    }
}