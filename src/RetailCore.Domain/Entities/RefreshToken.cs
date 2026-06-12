using RetailCore.Domain.Common;

namespace RetailCore.Domain.Entities;

public class RefreshToken : BaseEntity
{
    public long UserId { get; set; }
    public User? User { get; set; }

    public string Token { get; set; } = string.Empty;
    public DateTimeOffset ExpiresAt { get; set; }
    public DateTimeOffset? RevokedAt { get; set; }
    public DateTimeOffset CreatedAt { get; set; }

    public bool IsActive => RevokedAt is null && DateTimeOffset.UtcNow < ExpiresAt;
}
