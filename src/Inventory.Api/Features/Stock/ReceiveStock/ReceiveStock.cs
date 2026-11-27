using Inventory.Domain.Stock;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Inventory.Api.Features.Stock.ReceiveStock;

public sealed record ReceiveStockRequest(int Quantity);

public sealed record ReceiveStockResponse(string Sku, int OnHand);

/// <summary>Goods arrive at the warehouse. Creates the stock item the first time a SKU is received.</summary>
public static class ReceiveStock
{
    public static void Map(IEndpointRouteBuilder group) =>
        group.MapPost("/{sku}/receipts", Handle).WithName(nameof(ReceiveStock));

    internal static async Task<Ok<ReceiveStockResponse>> Handle(
        string sku, ReceiveStockRequest request, IReceiveStockStore store, CancellationToken ct)
    {
        var parsed = Sku.Parse(sku);
        var quantity = Quantity.From(request.Quantity);
        var loaded = await store.FindAsync(parsed, ct) ?? ReceiveStockStore.Loaded.New(StockItem.Create(parsed));
        loaded.Item.Receive(quantity);
        await store.SaveAsync(loaded, ct);
        return TypedResults.Ok(new ReceiveStockResponse(loaded.Item.Sku.Value, loaded.Item.OnHand));
    }
}
