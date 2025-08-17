using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RuminsterBackend.Exceptions;

namespace RuminsterBackend.Middleware
{
    public class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        private readonly RequestDelegate _next = next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger = logger;

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (NotFoundException ex)
            {
                _logger.LogWarning("Not found: {Message}", ex.Message);
                context.Response.StatusCode = StatusCodes.Status404NotFound;
                await context.Response.WriteAsJsonAsync(new { Error = ex.Message });
            }
            catch (AuthenticationException ex)
            {
                _logger.LogWarning("Unauthorized access: {Message}", ex.Message);
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsJsonAsync(new { Error = ex.Message });
            }
            catch (ForbiddenException ex)
            {
                _logger.LogWarning("Forbidden: {Message}", ex.Message);
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                await context.Response.WriteAsJsonAsync(new { Error = ex.Message });
            }
            catch (ValidationException ex)
            {
                _logger.LogWarning("Validation error: {Message}", ex.Message);
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                await context.Response.WriteAsJsonAsync(new { Error = ex.Message });
            }
            catch (IdentityOperationException ex)
            {
                _logger.LogWarning("Identity operation failed: {Message}", ex.Message);
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                await context.Response.WriteAsJsonAsync(new 
                { 
                    Error = ex.Message, 
                    Details = ex.Errors?.Select(e => e.Description) 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred.");
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                // Avoid leaking internal error details
                await context.Response.WriteAsJsonAsync(new { Error = "An unexpected error occurred." });
            }
        }
    }
}