namespace RuminsterBackend.Services.Interfaces
{
    public interface ITokenService
    {
        string GenerateAccessToken(string userId, IEnumerable<string> roles);

        string GenerateRefreshToken();

        int GetAccessTokenExpiry();
    }
}