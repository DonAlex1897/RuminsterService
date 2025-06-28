namespace RuminsterBackend.Services.Interfaces
{
    public interface IEmailService
    {
        Task SendEmailVerificationAsync(string email, string userName, string token);
        Task SendPasswordResetAsync(string email, string userName, string token);
    }
}
