using Inventory.Domain.Stock;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Inventory.Api.Features.Stock.IssueStock;

public sealed record IssueStockRequest(int Quantity);

public sealed record IssueStockResponse(string Sku, int OnHand);

/// <summary>Goods leave the warehouse. Stock on hand can never go negative (the domain enforces it).</summary>
public static class IssueStock
{
    public static void Map(IEndpointRouteBuilder group) =>
        group.MapPost("/{sku}/issues", Handle).WithName(nameof(IssueStock));

    internal static async Task<Results<Ok<IssueStockResponse>, NotFound>> Handle(
        string sku, IssueStockRequest request, IIssueStockStore store, CancellationToken ct)
    {
        var parsed = Sku.Parse(sku);
        var quantity = Quantity.From(request.Quantity);
        var loaded = await store.FindAsync(parsed, ct);
        if (loaded is null)
        {
            return TypedResults.NotFound();
        }

        loaded.Item.Issue(quantity);
        await store.SaveAsync(loaded, ct);
        return TypedResults.Ok(new IssueStockResponse(loaded.Item.Sku.Value, loaded.Item.OnHand));
    }
}
