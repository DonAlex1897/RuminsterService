using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RuminsterBackend.Data;
using RuminsterBackend.Models.DTOs.User;
using RuminsterBackend.Services.Interfaces;

namespace RuminsterBackend.Services
{
    public class RequestContextService(
        IUsersService usersService,
        RuminsterDbContext context
    ) : IRequestContextService
    {
        private readonly DateTime _time = DateTime.UtcNow;

        private readonly IUsersService _usersService = usersService;

        private UserResponse? _user;

        public UserResponse User
        {
            get
            {
                _user ??= _usersService.GetCurrentUserAsync().Result;

                return _user;
            }
        }

        public DateTime Time  => _time;

        public RuminsterDbContext Context { get; set; } = context;
    }
}