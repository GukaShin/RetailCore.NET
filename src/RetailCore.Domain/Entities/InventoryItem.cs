using RetailCore.Domain.Common;

namespace RetailCore.Domain.Entities;

/// <summary>Per-store stock level for a product. Guarded by a concurrency token to prevent overselling.</summary>
public class InventoryItem : BaseEntity
{
    public long StoreId { get; set; }
    public Store? Store { get; set; }

    public long ProductId { get; set; }
    public Product? Product { get; set; }

    public int Quantity { get; set; }
    public int ReservedQuantity { get; set; }
    public int LowStockThreshold { get; set; }

    /// <summary>Quantity available for sale (on-hand minus reserved).</summary>
    public int AvailableQuantity => Quantity - ReservedQuantity;

    /// <summary>
    /// PostgreSQL system column <c>xmin</c> mapped as an optimistic concurrency token.
    /// Postgres bumps this on every row update, so a stale write fails with a concurrency exception.
    /// </summary>
    public uint RowVersion { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }
}
