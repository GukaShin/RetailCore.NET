namespace RetailCore.Api.Authorization;

public static class RolePolicies
{
    public const string Admin = "Admin";
    public const string StoreManager = "StoreManager";
    public const string Cashier = "Cashier";
    public const string InventoryManager = "InventoryManager";

    public const string AdminOnly = "AdminOnly";
    public const string Management = "Management";
    public const string CatalogManagement = "CatalogManagement";
    public const string InventoryAccess = "InventoryAccess";
    public const string CashierOperations = "CashierOperations";
}
