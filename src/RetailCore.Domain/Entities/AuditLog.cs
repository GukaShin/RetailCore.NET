using RetailCore.Domain.Common;

namespace RetailCore.Domain.Entities;

public class AuditLog : BaseEntity
{
    public long? UserId { get; set; }
    public long? StoreId { get; set; }

    public string Action { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string EntityName { get; set; } = string.Empty;
    public string? EntityId { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}
