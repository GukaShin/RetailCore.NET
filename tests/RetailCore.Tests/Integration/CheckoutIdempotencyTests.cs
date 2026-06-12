using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using RetailCore.Contracts.Sales;

namespace RetailCore.Tests.Integration;

[Collection(nameof(IntegrationTestCollection))]
public class CheckoutIdempotencyTests
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly RetailCoreWebApplicationFactory _factory;

    public CheckoutIdempotencyTests(RetailCoreWebApplicationFactory factory) => _factory = factory;

    [Fact]
    public async Task Checkout_SameIdempotencyKeyFiveTimes_CreatesOneSale()
    {
        var ctx = await CheckoutTestHelper.PrepareCheckoutContextAsync(_factory, stockQuantity: 10);
        var key = $"checkout-idempotent-{Guid.NewGuid():N}";
        var request = CheckoutTestHelper.BuildCheckoutRequest(ctx, key);

        var responses = new List<HttpResponseMessage>();
        for (var i = 0; i < 5; i++)
        {
            responses.Add(await ctx.Client.PostAsJsonAsync("/api/sales/checkout", request));
        }

        responses.Should().OnlyContain(r => r.StatusCode == HttpStatusCode.OK);

        var bodies = new List<CheckoutResponse>();
        foreach (var response in responses)
        {
            var body = await response.Content.ReadFromJsonAsync<CheckoutResponse>(JsonOptions);
            body.Should().NotBeNull();
            bodies.Add(body!);
        }

        bodies.Select(b => b.SaleId).Distinct().Should().HaveCount(1);
        bodies.Select(b => b.ReceiptNumber).Distinct().Should().HaveCount(1);

        var saleCount = await CheckoutTestHelper.GetSaleCountAsync(_factory);
        saleCount.Should().Be(1);

        var finalStock = await CheckoutTestHelper.GetStockAsync(_factory, ctx.StoreId, ctx.ProductId);
        finalStock.Should().Be(9);
    }
}
