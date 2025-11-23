using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using RuminsterBackend.Data;
using RuminsterBackend.Data.DataSeed;
using RuminsterBackend.Middleware;
using RuminsterBackend.Models;
using RuminsterBackend.Services;
using RuminsterBackend.Services.Interfaces;
using System.Security.Claims;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddHealthChecks();

var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKeyString = jwtSettings["SecretKey"] ?? string.Empty;
// Enforce presence and minimum length for JWT secret to avoid weak keys
if (string.IsNullOrWhiteSpace(secretKeyString) || secretKeyString.Length < 32)
{
    throw new InvalidOperationException("JwtSettings:SecretKey must be set and at least 32 characters long.");
}
var secretKey = Encoding.UTF8.GetBytes(secretKeyString);

var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
if (allowedOrigins.Length == 0)
{
    throw new InvalidOperationException("Cors:AllowedOrigins must contain at least one origin");
}

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy
            .WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.AddIdentity<User, Role>()
    .AddEntityFrameworkStores<RuminsterDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings["Issuer"],
            ValidAudience = jwtSettings["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(secretKey),
            RoleClaimType = ClaimTypes.Role,
            // Eliminate default clock skew to tighten token validation window
            ClockSkew = TimeSpan.Zero,
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddHttpContextAccessor();

builder.Services.AddDbContext<RuminsterDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
           .UseSnakeCaseNamingConvention()
           .AddInterceptors(new RuminsterBackend.Interceptors.EntityLoggingInterceptor()));

builder.Services.AddScoped<IRequestContextService, RequestContextService>();
builder.Services.AddTransient<IAuthService, AuthService>();
builder.Services.AddTransient<IUsersService, UsersService>();
builder.Services.AddTransient<ITokenService, TokenService>();
builder.Services.AddTransient<IRuminationsService, RuminationsService>();
builder.Services.AddTransient<IUserRelationsService, UserRelationsService>();
builder.Services.AddTransient<IEmailService, EmailService>();
builder.Services.AddTransient<ITermsOfServiceService, TermsOfServiceService>();
builder.Services.AddTransient<ICommentsService, CommentsService>();
builder.Services.AddTransient<ITextSearchService, TextSearchService>();

// Configure Email Settings
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "Ruminster API", Version = "v1" });

    // Define the security scheme for JWT Bearer Authentication
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter 'Bearer <your_token>'"
    });

    // Require JWT token for API calls
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// Initialize roles and test data (if they don't exist)
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<RuminsterDbContext>();
    context.Database.Migrate(); // Applies any pending migrations

    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<Role>>();
    await RoleInitializer.InitializeRoles(roleManager);

    // Initialize test data in development environment
    if (app.Environment.IsDevelopment())
    {
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
        await TestDataInitializer.InitializeTestData(context, userManager);
    }
}

app.UseRouting();

// Add CORS middleware
app.UseCors("AllowFrontend");

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Only use HTTPS redirection in production
if (!app.Environment.IsDevelopment())
{
    // Add HSTS and basic security headers in production
    app.UseHsts();
    app.Use(async (context, next) =>
    {
        context.Response.Headers["X-Content-Type-Options"] = "nosniff";
        context.Response.Headers["X-Frame-Options"] = "DENY";
        context.Response.Headers["Referrer-Policy"] = "no-referrer";
        context.Response.Headers["X-XSS-Protection"] = "0"; // modern browsers ignore this; CSP is preferred
        await next();
    });
    app.UseHttpsRedirection();
}

app.MapControllers();
// app.MapControllers().RequireAuthorization(new AuthorizeAttribute() 
// { 
//     AuthenticationSchemes= JwtBearerDefaults.AuthenticationScheme,
//     Roles="Admin,Member,Moderator",
// });

app.MapHealthChecks("/health");

app.Run();

