namespace RetailCore.Domain.Common;

/// <summary>Base type for all persisted entities. Uses a 64-bit identity key.</summary>
public abstract class BaseEntity
{
    public long Id { get; set; }
}
