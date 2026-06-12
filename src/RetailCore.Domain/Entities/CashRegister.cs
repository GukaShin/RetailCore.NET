using RetailCore.Domain.Common;
using RetailCore.Domain.Enums;

namespace RetailCore.Domain.Entities;

public class CashRegister : BaseEntity
{
    public long StoreId { get; set; }
    public Store? Store { get; set; }

    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public RegisterStatus Status { get; set; } = RegisterStatus.Active;
    public DateTimeOffset CreatedAt { get; set; }
}
