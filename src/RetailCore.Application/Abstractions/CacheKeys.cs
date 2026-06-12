namespace RetailCore.Application.Abstractions;

/// <summary>Centralised Redis key builders so cache reads and invalidations stay consistent.</summary>
public static class CacheKeys
{
    public static string ProductByBarcode(string barcode) => $"cache:product:barcode:{barcode}";
    public static string LowStock(long storeId) => $"cache:lowstock:store:{storeId}";

    public const string RequestCounter = "counter:requests";
    public static string SalesTodayCounter(long storeId) => $"counter:sales:today:{storeId}";
    public static string RevenueTodayCounter(long storeId) => $"counter:revenue:today:{storeId}";
    public static string Idempotency(string key) => $"idempotency:checkout:{key}";
}
