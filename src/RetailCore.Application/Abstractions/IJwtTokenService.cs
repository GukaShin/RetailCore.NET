using RetailCore.Domain.Entities;

namespace RetailCore.Application.Abstractions;

public interface IJwtTokenService
{
    (string Token, DateTimeOffset ExpiresAt) GenerateAccessToken(User user);
    string GenerateRefreshToken();
}
