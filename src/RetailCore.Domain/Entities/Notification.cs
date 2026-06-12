using RetailCore.Domain.Common;

namespace RetailCore.Domain.Entities;

public class Notification : BaseEntity
{
    public long? UserId { get; set; }
    public long? StoreId { get; set; }

    public string Type { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public bool IsRead { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}
