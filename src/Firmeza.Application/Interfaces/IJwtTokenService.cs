namespace Firmeza.Application.Interfaces;

public interface IJwtTokenService
{
    (string Token, DateTime ExpiresAt) GenerateToken(string userId, string userName, string email, IEnumerable<string> roles);
}
