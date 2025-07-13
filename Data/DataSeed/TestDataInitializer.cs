using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RuminsterBackend.Models;
using RuminsterBackend.Models.Enums;

namespace RuminsterBackend.Data.DataSeed
{
    public static class TestDataInitializer
    {
        private static readonly Random _random = new Random();
        
        private static readonly string[] _firstNames = {
            "Alex", "Jordan", "Casey", "Taylor", "Morgan", "Riley", "Avery", "Quinn",
            "Blake", "Cameron", "Drew", "Emery", "Hayden", "Jamie", "Kendall", "Lane",
            "Sage", "River", "Rowan", "Skyler", "Finley", "Parker", "Reese", "Bailey"
        };
        
        private static readonly string[] _lastNames = {
            "Smith", "Johnson", "Williams", "Brown", "Jones", "Garcia", "Miller", "Davis",
            "Rodriguez", "Martinez", "Hernandez", "Lopez", "Gonzalez", "Wilson", "Anderson",
            "Thomas", "Taylor", "Moore", "Jackson", "Martin", "Lee", "Perez", "Thompson"
        };
        
        private static readonly string[] _ruminationTemplates = {
            "Today I realized that {0}. It's been on my mind for a while now.",
            "I've been thinking about {0} and how it affects my daily life.",
            "Sometimes I wonder why {0} happens so often in my experience.",
            "Had an interesting conversation about {0} with a friend today.",
            "Reflecting on {0} and what it means for my future goals.",
            "I'm grateful for {0} and the positive impact it has had.",
            "Struggling with {0} lately and looking for ways to improve.",
            "The concept of {0} has been fascinating me recently.",
            "I need to work on {0} more consistently in my routine.",
            "Discovered something new about {0} that changed my perspective."
        };
        
        private static readonly string[] _topics = {
            "personal growth", "relationships", "career development", "mindfulness",
            "creativity", "health and wellness", "learning new skills", "travel experiences",
            "family connections", "work-life balance", "financial planning", "self-reflection",
            "community involvement", "environmental awareness", "technology's impact",
            "cultural differences", "art and music", "cooking adventures", "outdoor activities",
            "reading habits", "meditation practice", "friendship dynamics", "time management"
        };

        public static async Task InitializeTestData(
            RuminsterDbContext context, 
            UserManager<User> userManager, 
            int userCount = 20, 
            int ruminationCount = 50, 
            int relationCount = 30)
        {
            // Check if test data already exists
            if (await context.Users.AnyAsync(u => u.UserName.StartsWith("testuser")))
            {
                Console.WriteLine("Test data already exists. Clearing existing test data first...");
                await ClearTestData(context, userManager);
            }

            Console.WriteLine("Initializing test data...");

            // Create users
            var users = await CreateTestUsers(userManager, userCount);
            
            // Create ruminations
            await CreateTestRuminations(context, users, ruminationCount);
            
            // Create user relations
            await CreateTestUserRelations(context, users, relationCount);

            Console.WriteLine($"Test data initialization complete! Created {userCount} users, {ruminationCount} ruminations, and {relationCount} relations.");
        }

        public static async Task ClearTestData(RuminsterDbContext context, UserManager<User> userManager)
        {
            Console.WriteLine("Clearing existing test data...");

            // Remove related data first (due to foreign key constraints)
            var testUsers = await context.Users.Where(u => u.UserName.StartsWith("testuser")).ToListAsync();
            var testUserIds = testUsers.Select(u => u.Id).ToList();

            // Remove rumination audiences
            var ruminationAudiences = await context.RuminationAudiences
                .Where(ra => context.Ruminations.Any(r => testUserIds.Contains(r.CreateById) && r.Id == ra.RuminationId))
                .ToListAsync();
            context.RuminationAudiences.RemoveRange(ruminationAudiences);

            // Remove user relations
            var userRelations = await context.UserRelations
                .Where(ur => testUserIds.Contains(ur.InitiatorId) || testUserIds.Contains(ur.ReceiverId))
                .ToListAsync();
            context.UserRelations.RemoveRange(userRelations);

            // Remove ruminations
            var ruminations = await context.Ruminations
                .Where(r => testUserIds.Contains(r.CreateById))
                .ToListAsync();
            context.Ruminations.RemoveRange(ruminations);

            await context.SaveChangesAsync();

            // Remove users using UserManager
            foreach (var user in testUsers)
            {
                var refreshTokens = await context.RefreshTokens
                    .Where(ut => ut.UserId == user.Id)
                    .ToListAsync();
                context.RefreshTokens.RemoveRange(refreshTokens);
                await userManager.DeleteAsync(user);
            }

            Console.WriteLine($"Cleared {testUsers.Count} test users and their associated data.");
        }

        private static async Task<List<User>> CreateTestUsers(UserManager<User> userManager, int count)
        {
            var users = new List<User>();

            for (int i = 0; i < count; i++)
            {
                var firstName = _firstNames[_random.Next(_firstNames.Length)];
                var lastName = _lastNames[_random.Next(_lastNames.Length)];
                var username = $"testuser{i:D3}";
                var email = $"{username}@example.com";

                var user = new User
                {
                    UserName = username,
                    Email = email,
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(user, "TestPassword123!");
                
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, "User");
                    users.Add(user);
                    Console.WriteLine($"Created user: {username}");
                }
                else
                {
                    Console.WriteLine($"Failed to create user {username}: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                }
            }

            return users;
        }

        private static async Task CreateTestRuminations(RuminsterDbContext context, List<User> users, int count)
        {
            var ruminations = new List<Rumination>();

            for (int i = 0; i < count; i++)
            {
                var user = users[_random.Next(users.Count)];
                var topic = _topics[_random.Next(_topics.Length)];
                var template = _ruminationTemplates[_random.Next(_ruminationTemplates.Length)];
                var content = string.Format(template, topic);
                
                var createdDate = DateTime.SpecifyKind(DateTime.UtcNow.AddDays(-_random.Next(0, 30)).AddHours(-_random.Next(0, 24)), DateTimeKind.Utc);

                var rumination = new Rumination
                {
                    Content = content,
                    IsPublished = _random.Next(0, 10) > 2, // 80% chance of being published
                    CreateById = user.Id,
                    UpdateById = user.Id,
                    CreateTMS = createdDate,
                    UpdateTMS = createdDate,
                    IsDeleted = false
                };

                ruminations.Add(rumination);
            }

            context.Ruminations.AddRange(ruminations);
            await context.SaveChangesAsync();

            // Add audiences for published ruminations
            await CreateRuminationAudiences(context, ruminations, users);

            Console.WriteLine($"Created {count} ruminations");
        }

        private static async Task CreateRuminationAudiences(RuminsterDbContext context, List<Rumination> ruminations, List<User> users)
        {
            var audiences = new List<RuminationAudience>();
            var relationTypes = Enum.GetValues<UserRelationType>();

            foreach (var rumination in ruminations.Where(r => r.IsPublished))
            {
                // Each rumination gets 1-3 random audience types
                var audienceCount = _random.Next(1, 4);
                var selectedTypes = relationTypes.OrderBy(x => _random.Next()).Take(audienceCount);

                foreach (var relationType in selectedTypes)
                {
                    audiences.Add(new RuminationAudience
                    {
                        RuminationId = rumination.Id,
                        RelationType = relationType,
                        CreateTMS = rumination.CreateTMS,
                        UpdateTMS = rumination.UpdateTMS,
                    });
                }
            }

            context.RuminationAudiences.AddRange(audiences);
            await context.SaveChangesAsync();
        }

        private static async Task CreateTestUserRelations(RuminsterDbContext context, List<User> users, int count)
        {
            var relations = new List<UserRelation>();
            var relationTypes = Enum.GetValues<UserRelationType>();
            var createdRelations = new HashSet<string>(); // To avoid duplicate relations

            for (int i = 0; i < count; i++)
            {
                var initiator = users[_random.Next(users.Count)];
                var receiver = users[_random.Next(users.Count)];

                // Ensure different users and no duplicate relations
                if (initiator.Id == receiver.Id)
                    continue;

                var relationKey = $"{initiator.Id}-{receiver.Id}";
                var reverseKey = $"{receiver.Id}-{initiator.Id}";

                if (createdRelations.Contains(relationKey) || createdRelations.Contains(reverseKey))
                    continue;

                var relationType = relationTypes[_random.Next(relationTypes.Length)];
                var createdDate = DateTime.SpecifyKind(DateTime.UtcNow.AddDays(-_random.Next(0, 60)), DateTimeKind.Utc);
                var isAccepted = _random.Next(0, 10) > 2; // 80% chance of being accepted

                var relation = new UserRelation
                {
                    InitiatorId = initiator.Id,
                    ReceiverId = receiver.Id,
                    Type = relationType,
                    IsAccepted = isAccepted,
                    IsRejected = !isAccepted && _random.Next(0, 10) > 7, // 30% of unaccepted are rejected
                    CreateById = initiator.Id,
                    UpdateById = initiator.Id,
                    CreateTMS = createdDate,
                    UpdateTMS = createdDate,
                    IsDeleted = false
                };

                relations.Add(relation);
                createdRelations.Add(relationKey);
            }

            context.UserRelations.AddRange(relations);
            await context.SaveChangesAsync();

            Console.WriteLine($"Created {relations.Count} user relations");
        }
    }
}
