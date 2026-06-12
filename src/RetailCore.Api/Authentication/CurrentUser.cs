using System.Security.Claims;
using RetailCore.Application.Abstractions;
using RetailCore.Domain.Enums;
using RetailCore.Infrastructure.Security;

namespace RetailCore.Api.Authentication;

/// <summary>Reads the authenticated principal from the current HTTP request.</summary>
public class CurrentUser : ICurrentUser
{
    private readonly IHttpContextAccessor _accessor;

    public CurrentUser(IHttpContextAccessor accessor) => _accessor = accessor;

    private ClaimsPrincipal? Principal => _accessor.HttpContext?.User;

    public bool IsAuthenticated => Principal?.Identity?.IsAuthenticated ?? false;

    public long? UserId =>
        long.TryParse(Principal?.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : null;

    public long? StoreId =>
        long.TryParse(Principal?.FindFirstValue(JwtTokenService.StoreIdClaim), out var id) ? id : null;

    public UserRole? Role =>
        Enum.TryParse<UserRole>(Principal?.FindFirstValue(ClaimTypes.Role), out var role) ? role : null;

    public string? Email => Principal?.FindFirstValue(ClaimTypes.Email);
}
