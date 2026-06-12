using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RetailCore.Application.Abstractions;
using RetailCore.Application.Common.Exceptions;
using RetailCore.Contracts.Sales;
using RetailCore.Domain.Entities;
using RetailCore.Domain.Enums;
using RetailCore.Infrastructure.Persistence;

namespace RetailCore.Infrastructure.Services;

public class SaleService : ISaleService
{
    private static readonly TimeSpan IdempotencyTtl = TimeSpan.FromHours(24);
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly RetailCoreDbContext _db;
    private readonly ICartCalculationService _cart;
    private readonly ICacheService _cache;
    private readonly ICurrentUser _currentUser;
    private readonly IDateTimeProvider _clock;
    private readonly ILogger<SaleService> _logger;

    public SaleService(
        RetailCoreDbContext db,
        ICartCalculationService cart,
        ICacheService cache,
        ICurrentUser currentUser,
        IDateTimeProvider clock,
        ILogger<SaleService> logger)
    {
        _db = db;
        _cart = cart;
        _cache = cache;
        _currentUser = currentUser;
        _clock = clock;
        _logger = logger;
    }

    public async Task<CheckoutResponse> CheckoutAsync(CheckoutRequest request, CancellationToken ct = default)
    {
        var idempotencyKey = request.IdempotencyKey.Trim();
        if (string.IsNullOrWhiteSpace(idempotencyKey))
        {
            throw new BusinessRuleException("Idempotency key is required.");
        }

        var cached = await _cache.GetAsync<CheckoutResponse>(CacheKeys.Idempotency(idempotencyKey), ct);
        if (cached is not null)
        {
            _logger.LogInformation("Returning cached checkout result for idempotency key {Key}", idempotencyKey);
            return cached;
        }

        var existing = await _db.IdempotencyRecords
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Key == idempotencyKey && r.ResponsePayload != null, ct);

        if (existing?.ResponsePayload is not null)
        {
            var prior = JsonSerializer.Deserialize<CheckoutResponse>(existing.ResponsePayload, JsonOptions)!;
            await _cache.SetAsync(CacheKeys.Idempotency(idempotencyKey), prior, IdempotencyTtl, ct);
            return prior;
        }

        var cashierId = _currentUser.UserId ?? throw new UnauthorizedException("Not authenticated.");

        var shift = await _db.CashierShifts.FirstOrDefaultAsync(s => s.Id == request.ShiftId, ct)
            ?? throw new NotFoundException("Shift", request.ShiftId);

        if (shift.Status != ShiftStatus.Open)
        {
            throw new BusinessRuleException("Cashier must have an open shift before creating a sale.");
        }

        if (shift.CashierId != cashierId)
        {
            throw new BusinessRuleException("Shift does not belong to the current cashier.");
        }

        if (shift.StoreId != request.StoreId)
        {
            throw new BusinessRuleException("Shift store does not match checkout store.");
        }

        if (request.Items.Count == 0)
        {
            throw new BusinessRuleException("A sale must contain at least one product.");
        }

        var productIds = request.Items.Select(i => i.ProductId).Distinct().OrderBy(id => id).ToList();

        var products = await _db.Products
            .Where(p => productIds.Contains(p.Id) && p.IsActive)
            .ToDictionaryAsync(p => p.Id, ct);

        if (products.Count != productIds.Count)
        {
            var missing = productIds.Except(products.Keys).First();
            throw new NotFoundException("Product", missing);
        }

        var cartLines = request.Items.Select(item =>
        {
            var product = products[item.ProductId];
            return new CartLineInput(
                product.Id,
                product.Name,
                product.Barcode,
                product.Price,
                product.VatPercent,
                item.Quantity);
        }).ToList();

        var calculation = _cart.Calculate(cartLines, request.Payments);

        await using var transaction = await _db.Database.BeginTransactionAsync(ct);

        try
        {
            // Lock inventory rows in deterministic order to prevent deadlocks and overselling.
            foreach (var productId in productIds)
            {
                await _db.Database.ExecuteSqlInterpolatedAsync(
                    $"""SELECT 1 FROM inventory_items WHERE "StoreId" = {request.StoreId} AND "ProductId" = {productId} FOR UPDATE""",
                    ct);
            }

            var inventoryByProduct = await _db.InventoryItems
                .Where(i => i.StoreId == request.StoreId && productIds.Contains(i.ProductId))
                .ToDictionaryAsync(i => i.ProductId, ct);

            foreach (var productId in productIds)
            {
                if (!inventoryByProduct.ContainsKey(productId))
                {
                    var qty = request.Items.First(i => i.ProductId == productId).Quantity;
                    throw new InsufficientStockException(productId, qty, 0);
                }
            }

            foreach (var item in request.Items)
            {
                var inventory = inventoryByProduct[item.ProductId];
                var available = inventory.Quantity - inventory.ReservedQuantity;
                if (available < item.Quantity)
                {
                    throw new InsufficientStockException(item.ProductId, item.Quantity, available);
                }
            }

            var now = _clock.UtcNow;
            var receiptNumber = await GenerateReceiptNumberAsync(now, ct);

            var sale = new Sale
            {
                StoreId = request.StoreId,
                CashierId = cashierId,
                ShiftId = request.ShiftId,
                CashRegisterId = request.CashRegisterId,
                CustomerId = request.CustomerId,
                ReceiptNumber = receiptNumber,
                SubtotalAmount = calculation.Subtotal,
                DiscountAmount = calculation.DiscountAmount,
                VatAmount = calculation.VatAmount,
                TotalAmount = calculation.Total,
                PaymentStatus = SalePaymentStatus.Paid,
                SaleStatus = SaleStatus.Completed,
                CreatedAt = now,
                CompletedAt = now
            };

            _db.Sales.Add(sale);
            await _db.SaveChangesAsync(ct);

            foreach (var line in calculation.Lines)
            {
                _db.SaleItems.Add(new SaleItem
                {
                    SaleId = sale.Id,
                    ProductId = line.ProductId,
                    ProductNameSnapshot = line.Name,
                    BarcodeSnapshot = line.Barcode,
                    Quantity = line.Quantity,
                    UnitPrice = line.UnitPrice,
                    DiscountAmount = line.DiscountAmount,
                    VatAmount = line.VatAmount,
                    LineTotal = line.LineTotal
                });
            }

            foreach (var payment in request.Payments)
            {
                _db.Payments.Add(new Payment
                {
                    SaleId = sale.Id,
                    PaymentMethod = payment.PaymentMethod,
                    Amount = payment.Amount,
                    Status = PaymentStatus.Succeeded,
                    CreatedAt = now,
                    ProcessedAt = now
                });
            }

            foreach (var item in request.Items)
            {
                var inventory = inventoryByProduct[item.ProductId];
                inventory.Quantity -= item.Quantity;
                inventory.UpdatedAt = now;

                _db.StockMovements.Add(new StockMovement
                {
                    StoreId = request.StoreId,
                    ProductId = item.ProductId,
                    QuantityChange = -item.Quantity,
                    MovementType = StockMovementType.Sale,
                    Reason = $"Sale {receiptNumber}",
                    ReferenceId = sale.Id,
                    CreatedByUserId = cashierId,
                    CreatedAt = now
                });
            }

            var receiptText = BuildReceiptText(sale, calculation, shift);
            _db.Receipts.Add(new Receipt
            {
                SaleId = sale.Id,
                ReceiptNumber = receiptNumber,
                ReceiptText = receiptText,
                CreatedAt = now
            });

            _db.AuditLogs.Add(new AuditLog
            {
                UserId = cashierId,
                StoreId = request.StoreId,
                Action = "CheckoutCompleted",
                Description = $"Sale {receiptNumber} completed for {calculation.Total:F2}",
                EntityName = nameof(Sale),
                EntityId = sale.Id.ToString(),
                CreatedAt = now
            });

            var response = new CheckoutResponse(
                sale.Id,
                receiptNumber,
                calculation.Subtotal,
                calculation.DiscountAmount,
                calculation.VatAmount,
                calculation.Total,
                calculation.Paid,
                calculation.Change,
                SaleStatus.Completed.ToString());

            var payload = JsonSerializer.Serialize(response, JsonOptions);
            _db.IdempotencyRecords.Add(new IdempotencyRecord
            {
                Key = idempotencyKey,
                SaleId = sale.Id,
                ResponsePayload = payload,
                CreatedAt = now,
                ExpiresAt = now.Add(IdempotencyTtl)
            });

            await _db.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);

            await _cache.SetAsync(CacheKeys.Idempotency(idempotencyKey), response, IdempotencyTtl, ct);
            await _cache.IncrementAsync(CacheKeys.SalesTodayCounter(request.StoreId), 1, ct);
            await _cache.IncrementAsync(CacheKeys.RevenueTodayCounter(request.StoreId), (long)(calculation.Total * 100), ct);
            await _cache.RemoveAsync(CacheKeys.LowStock(request.StoreId), ct);

            _logger.LogInformation(
                "Checkout completed: sale {SaleId}, receipt {Receipt}, total {Total}",
                sale.Id, receiptNumber, calculation.Total);

            return response;
        }
        catch (DbUpdateException ex) when (IsIdempotencyConflict(ex))
        {
            await transaction.RollbackAsync(ct);
            var replay = await _db.IdempotencyRecords
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.Key == idempotencyKey, ct);

            if (replay?.ResponsePayload is not null)
            {
                return JsonSerializer.Deserialize<CheckoutResponse>(replay.ResponsePayload, JsonOptions)!;
            }

            throw;
        }
        catch
        {
            await transaction.RollbackAsync(ct);
            throw;
        }
    }

    public async Task<IReadOnlyList<SaleDto>> GetSalesAsync(long? storeId, CancellationToken ct = default)
    {
        var query = _db.Sales.AsQueryable();
        if (storeId is { } sid)
        {
            query = query.Where(s => s.StoreId == sid);
        }

        return await query
            .OrderByDescending(s => s.CreatedAt)
            .Select(s => new SaleDto(
                s.Id,
                s.ReceiptNumber,
                s.StoreId,
                s.CashierId,
                s.SubtotalAmount,
                s.DiscountAmount,
                s.VatAmount,
                s.TotalAmount,
                s.PaymentStatus.ToString(),
                s.SaleStatus.ToString(),
                s.CreatedAt,
                s.CompletedAt))
            .ToListAsync(ct);
    }

    public async Task<SaleDetailDto> GetByIdAsync(long id, CancellationToken ct = default)
    {
        var sale = await _db.Sales
            .Include(s => s.Items)
            .Include(s => s.Payments)
            .FirstOrDefaultAsync(s => s.Id == id, ct)
            ?? throw new NotFoundException("Sale", id);

        return new SaleDetailDto(
            sale.Id,
            sale.ReceiptNumber,
            sale.StoreId,
            sale.CashierId,
            sale.SubtotalAmount,
            sale.DiscountAmount,
            sale.VatAmount,
            sale.TotalAmount,
            sale.PaymentStatus.ToString(),
            sale.SaleStatus.ToString(),
            sale.CreatedAt,
            sale.CompletedAt,
            sale.Items.Select(i => new SaleItemDto(
                i.Id, i.ProductId, i.ProductNameSnapshot, i.BarcodeSnapshot,
                i.Quantity, i.UnitPrice, i.DiscountAmount, i.VatAmount, i.LineTotal)).ToList(),
            sale.Payments.Select(p => new PaymentDto(
                p.Id, p.PaymentMethod.ToString(), p.Amount, p.Status.ToString(), p.CreatedAt)).ToList());
    }

    public async Task<ReceiptDto> GetReceiptAsync(long saleId, CancellationToken ct = default)
    {
        var receipt = await _db.Receipts.FirstOrDefaultAsync(r => r.SaleId == saleId, ct)
            ?? throw new NotFoundException($"Receipt for sale {saleId} was not found.");

        return new ReceiptDto(receipt.Id, receipt.SaleId, receipt.ReceiptNumber, receipt.ReceiptText, receipt.CreatedAt);
    }

    private async Task<string> GenerateReceiptNumberAsync(DateTimeOffset now, CancellationToken ct)
    {
        var prefix = $"RCPT-{now.Year}-";
        var lastNumber = await _db.Sales
            .Where(s => s.ReceiptNumber.StartsWith(prefix))
            .OrderByDescending(s => s.ReceiptNumber)
            .Select(s => s.ReceiptNumber)
            .FirstOrDefaultAsync(ct);

        var sequence = 1;
        if (lastNumber is not null && lastNumber.Length > prefix.Length)
        {
            var suffix = lastNumber[prefix.Length..];
            if (int.TryParse(suffix, out var parsed))
            {
                sequence = parsed + 1;
            }
        }

        return $"{prefix}{sequence:D6}";
    }

    private static string BuildReceiptText(
        Sale sale,
        CartCalculationResult calculation,
        CashierShift shift)
    {
        var lines = new List<string>
        {
            "=== RetailCore.NET ===",
            $"Receipt: {sale.ReceiptNumber}",
            $"Date: {sale.CreatedAt:yyyy-MM-dd HH:mm}",
            $"Shift: {shift.Id}",
            "---",
        };

        foreach (var line in calculation.Lines)
        {
            lines.Add($"{line.Name} x{line.Quantity} @ {line.UnitPrice:F2} = {line.LineTotal:F2}");
        }

        lines.Add("---");
        lines.Add($"Subtotal: {calculation.Subtotal:F2}");
        if (calculation.DiscountAmount > 0)
        {
            lines.Add($"Discount: -{calculation.DiscountAmount:F2}");
        }
        lines.Add($"VAT: {calculation.VatAmount:F2}");
        lines.Add($"TOTAL: {calculation.Total:F2}");
        lines.Add($"Paid: {calculation.Paid:F2}");
        if (calculation.Change > 0)
        {
            lines.Add($"Change: {calculation.Change:F2}");
        }
        lines.Add("Thank you for shopping!");

        return string.Join(Environment.NewLine, lines);
    }

    private static bool IsIdempotencyConflict(DbUpdateException ex)
        => ex.InnerException?.Message.Contains("idempotency_records", StringComparison.OrdinalIgnoreCase) == true
           || ex.InnerException?.Message.Contains("duplicate key", StringComparison.OrdinalIgnoreCase) == true;
}
