using RetailCore.Domain.Common;

namespace RetailCore.Domain.Entities;

/// <summary>
/// Persisted record of a processed idempotency key so repeated checkout requests
/// return the original result instead of creating a duplicate sale.
/// The unique index on <see cref="Key"/> is the database-level guarantee.
/// </summary>
public class IdempotencyRecord : BaseEntity
{
    public string Key { get; set; } = string.Empty;

    /// <summary>The sale created by the original request.</summary>
    public long? SaleId { get; set; }

    /// <summary>Cached JSON response returned for repeat requests.</summary>
    public string? ResponsePayload { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset ExpiresAt { get; set; }
}
