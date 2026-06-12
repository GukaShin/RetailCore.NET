namespace RetailCore.Contracts.Inventory;

public record InventoryItemDto(
    long ProductId,
    string ProductName,
    string Barcode,
    int Quantity,
    int ReservedQuantity,
    int AvailableQuantity,
    int LowStockThreshold,
    bool IsLowStock);

/// <summary>Manual stock adjustment. Positive increases stock, negative decreases it.</summary>
public record StockAdjustRequest(long ProductId, int QuantityChange, string Reason);

public record ProductStockDto(long StoreId, string StoreName, int Quantity, int AvailableQuantity);
