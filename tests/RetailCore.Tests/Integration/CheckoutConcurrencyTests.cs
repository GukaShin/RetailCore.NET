using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using RetailCore.Contracts.Sales;

namespace RetailCore.Tests.Integration;

[Collection(nameof(IntegrationTestCollection))]
public class CheckoutConcurrencyTests
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly RetailCoreWebApplicationFactory _factory;

    public CheckoutConcurrencyTests(RetailCoreWebApplicationFactory factory) => _factory = factory;

    [Fact]
    public async Task Checkout_StockOne_TwoParallelCheckouts_OnlyOneSucceeds()
    {
        var runId = Guid.NewGuid().ToString("N");
        var ctx = await CheckoutTestHelper.PrepareCheckoutContextAsync(_factory, stockQuantity: 1);
        var ctxB = await CheckoutTestHelper.PrepareSecondCashierContextAsync(_factory, ctx);

        var requestA = CheckoutTestHelper.BuildCheckoutRequest(ctx, $"race-{runId}-a");
        var requestB = CheckoutTestHelper.BuildCheckoutRequest(ctxB, $"race-{runId}-b");

        var taskA = ctx.Client.PostAsJsonAsync("/api/sales/checkout", requestA);
        var taskB = ctxB.Client.PostAsJsonAsync("/api/sales/checkout", requestB);
        await Task.WhenAll(taskA, taskB);

        var responseA = await taskA;
        var responseB = await taskB;

        var statuses = new[] { responseA.StatusCode, responseB.StatusCode };
        statuses.Should().Contain(HttpStatusCode.OK);
        statuses.Should().Contain(HttpStatusCode.Conflict);

        var finalStock = await CheckoutTestHelper.GetStockAsync(_factory, ctx.StoreId, ctx.ProductId);
        finalStock.Should().Be(0);

        var saleCount = await CheckoutTestHelper.GetSaleCountAsync(_factory);
        saleCount.Should().Be(1);
    }

    [Fact]
    public async Task Checkout_StockHundred_ParallelCheckouts_AllSucceed()
    {
        const int stock = 100;
        var runId = Guid.NewGuid().ToString("N");
        var ctx = await CheckoutTestHelper.PrepareCheckoutContextAsync(_factory, stockQuantity: stock);

        // Same cashier/shift; unique idempotency keys. Row locks serialize stock decrements.
        var tasks = Enumerable.Range(0, stock).Select(i =>
        {
            var request = CheckoutTestHelper.BuildCheckoutRequest(ctx, $"parallel-{runId}-{i}");
            return ctx.Client.PostAsJsonAsync("/api/sales/checkout", request);
        }).ToArray();

        var responses = await Task.WhenAll(tasks);

        responses.Count(r => r.StatusCode == HttpStatusCode.OK).Should().Be(stock);
        responses.Count(r => r.StatusCode == HttpStatusCode.Conflict).Should().Be(0);

        var finalStock = await CheckoutTestHelper.GetStockAsync(_factory, ctx.StoreId, ctx.ProductId);
        finalStock.Should().Be(0);

        var saleCount = await CheckoutTestHelper.GetSaleCountAsync(_factory);
        saleCount.Should().Be(stock);
    }
}
