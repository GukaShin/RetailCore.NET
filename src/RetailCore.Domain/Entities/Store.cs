using RetailCore.Domain.Common;
using RetailCore.Domain.Enums;

namespace RetailCore.Domain.Entities;

public class Store : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public StoreStatus Status { get; set; } = StoreStatus.Active;
    public DateTimeOffset CreatedAt { get; set; }

    public ICollection<CashRegister> CashRegisters { get; set; } = new List<CashRegister>();
}
