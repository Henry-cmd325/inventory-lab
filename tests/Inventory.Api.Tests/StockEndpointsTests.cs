using System.Net;
using System.Net.Http.Json;
using CsCheck;
using Inventory.Api.Features.Stock.IssueStock;
using Inventory.Api.Features.Stock.ReceiveStock;
using Inventory.Domain.Stock;
using Microsoft.AspNetCore.Mvc;

namespace Inventory.Api.Tests;

public class StockEndpointsTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private readonly HttpClient client = factory.CreateClient();

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Fact]
    public async Task Receiving_a_new_sku_creates_it()
    {
        var sku = ApiFactory.NewSku();
        var response = await client.PostAsJsonAsync($"/stock/{sku}/receipts", new ReceiveStockRequest(5), Ct);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(new ReceiveStockResponse(sku, 5), await response.Content.ReadFromJsonAsync<ReceiveStockResponse>(Ct));
    }

    [Fact]
    public async Task Sku_in_the_url_is_normalised()
    {
        var sku = ApiFactory.NewSku();
        await Receive(sku.ToLowerInvariant(), 3);
        var response = await client.PostAsJsonAsync($"/stock/{sku}/issues", new IssueStockRequest(1), Ct);

        Assert.Equal(new IssueStockResponse(sku, 2), await response.Content.ReadFromJsonAsync<IssueStockResponse>(Ct));
    }

    [Fact]
    public async Task Issuing_more_than_on_hand_answers_409_and_keeps_the_stock()
    {
        var sku = ApiFactory.NewSku();
        await Receive(sku, 2);

        var response = await client.PostAsJsonAsync($"/stock/{sku}/issues", new IssueStockRequest(3), Ct);
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>(Ct);
        Assert.Equal("Insufficient stock", problem?.Title);

        var after = await client.PostAsJsonAsync($"/stock/{sku}/issues", new IssueStockRequest(2), Ct);
        Assert.Equal(new IssueStockResponse(sku, 0), await after.Content.ReadFromJsonAsync<IssueStockResponse>(Ct));
    }

    [Fact]
    public async Task Issuing_an_unknown_sku_answers_404()
    {
        var response = await client.PostAsJsonAsync($"/stock/{ApiFactory.NewSku()}/issues", new IssueStockRequest(1), Ct);
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Theory]
    [InlineData("receipts", "OK-1", 0)]
    [InlineData("receipts", "OK-1", Quantity.Max + 1)]
    [InlineData("issues", "OK-1", -1)]
    [InlineData("receipts", "BAD_SKU", 1)]
    public async Task Broken_rules_answer_400(string movement, string sku, int quantity)
    {
        var response = await client.PostAsJsonAsync($"/stock/{sku}/{movement}", new ReceiveStockRequest(quantity), Ct);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Concurrent_issues_never_take_stock_below_zero()
    {
        var sku = ApiFactory.NewSku();
        await Receive(sku, 10);

        var responses = await Task.WhenAll(Enumerable.Range(0, 25).Select(_ =>
            client.PostAsJsonAsync($"/stock/{sku}/issues", new IssueStockRequest(1), Ct)));

        var succeeded = responses.Count(r => r.StatusCode == HttpStatusCode.OK);
        Assert.All(responses, r => Assert.Contains(r.StatusCode, new[] { HttpStatusCode.OK, HttpStatusCode.Conflict }));
        Assert.True(succeeded <= 10);

        var rest = 10 - succeeded;
        if (rest > 0)
        {
            var final = await client.PostAsJsonAsync($"/stock/{sku}/issues", new IssueStockRequest(rest), Ct);
            Assert.Equal(new IssueStockResponse(sku, 0), await final.Content.ReadFromJsonAsync<IssueStockResponse>(Ct));
        }
    }

    [Fact]
    public void Any_sequence_of_movements_over_http_matches_the_model() =>
        Movements.Sample(CheckAgainstModel, iter: 30);

    /// <summary>Run 1 found this: an invalid quantity on an unknown SKU answered 404 instead of 400.</summary>
    [Fact]
    public void Regression_invalid_quantity_on_unknown_sku_is_400() =>
        Movements.Sample(CheckAgainstModel, seed: "00008nXqRM-4", iter: 1);

    private static readonly Gen<(bool Receive, int Quantity)[]> Movements = Gen.Select(Gen.Bool, Gen.Int[-2, 20]).Array[1, 15];

    private void CheckAgainstModel((bool Receive, int Quantity)[] ops)
        {
            var sku = ApiFactory.NewSku();
            var model = 0;
            var exists = false;
            foreach (var (receive, quantity) in ops)
            {
                var path = receive ? "receipts" : "issues";
                var response = client.PostAsJsonAsync($"/stock/{sku}/{path}", new ReceiveStockRequest(quantity), Ct).GetAwaiter().GetResult();

                HttpStatusCode expected;
                if (quantity < 1) expected = HttpStatusCode.BadRequest;
                else if (!receive && !exists) expected = HttpStatusCode.NotFound;
                else if (!receive && quantity > model) expected = HttpStatusCode.Conflict;
                else expected = HttpStatusCode.OK;

                Assert.Equal(expected, response.StatusCode);
                if (expected == HttpStatusCode.OK)
                {
                    model += receive ? quantity : -quantity;
                    exists = true;
                    var body = response.Content.ReadFromJsonAsync<ReceiveStockResponse>(Ct).GetAwaiter().GetResult();
                    Assert.Equal(model, body!.OnHand);
                }
            }
        }

    private async Task Receive(string sku, int quantity)
    {
        var response = await client.PostAsJsonAsync($"/stock/{sku}/receipts", new ReceiveStockRequest(quantity), Ct);
        response.EnsureSuccessStatusCode();
    }
}
