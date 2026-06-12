namespace RetailCore.Infrastructure.Security;

public class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Issuer { get; set; } = "RetailCore";
    public string Audience { get; set; } = "RetailCoreClients";

    /// <summary>HMAC signing key. Must be at least 32 chars. Override via configuration/secret in production.</summary>
    public string SigningKey { get; set; } = "dev-only-super-secret-signing-key-change-me-please-32+";

    public int AccessTokenMinutes { get; set; } = 30;
    public int RefreshTokenDays { get; set; } = 7;
}
