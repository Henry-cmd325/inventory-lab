using Inventory.Api.Infrastructure;
using Inventory.Domain.Stock;
using Npgsql;

namespace Inventory.Api.Features.Stock.IssueStock;

public interface IIssueStockStore
{
    Task<IssueStockStore.Loaded?> FindAsync(Sku sku, CancellationToken ct);

    Task SaveAsync(IssueStockStore.Loaded loaded, CancellationToken ct);
}

public sealed class IssueStockStore(NpgsqlDataSource db) : IIssueStockStore
{
    /// <summary>The item plus the row version read from the database.</summary>
    public sealed record Loaded(StockItem Item, int Version);

    public async Task<Loaded?> FindAsync(Sku sku, CancellationToken ct)
    {
        await using var command = db.CreateCommand("SELECT on_hand, version FROM stock_items WHERE sku = $1");
        command.Parameters.Add(new NpgsqlParameter<string> { TypedValue = sku.Value });
        await using var reader = await command.ExecuteReaderAsync(ct);
        if (!await reader.ReadAsync(ct))
        {
            return null;
        }

        return new Loaded(StockItem.Restore(sku, reader.GetInt32(0)), reader.GetInt32(1));
    }

    public async Task SaveAsync(Loaded loaded, CancellationToken ct)
    {
        await using var command = db.CreateCommand(
            "UPDATE stock_items SET on_hand = $2, version = version + 1 WHERE sku = $1 AND version = $3");
        command.Parameters.Add(new NpgsqlParameter<string> { TypedValue = loaded.Item.Sku.Value });
        command.Parameters.Add(new NpgsqlParameter<int> { TypedValue = loaded.Item.OnHand });
        command.Parameters.Add(new NpgsqlParameter<int> { TypedValue = loaded.Version });

        if (await command.ExecuteNonQueryAsync(ct) != 1)
        {
            throw new ConcurrencyConflictException($"Stock of {loaded.Item.Sku} changed while issuing. Try again.");
        }
    }
}
