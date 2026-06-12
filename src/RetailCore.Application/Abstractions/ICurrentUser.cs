using RetailCore.Domain.Enums;

namespace RetailCore.Application.Abstractions;

/// <summary>Provides access to the authenticated user for the current request.</summary>
public interface ICurrentUser
{
    bool IsAuthenticated { get; }
    long? UserId { get; }
    long? StoreId { get; }
    UserRole? Role { get; }
    string? Email { get; }
}
