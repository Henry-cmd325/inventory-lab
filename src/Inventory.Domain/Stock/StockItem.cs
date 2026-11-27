namespace Inventory.Domain.Stock;

/// <summary>
/// The stock of one SKU in the warehouse.
/// Rules:
/// <list type="number">
/// <item>Stock on hand can never go negative.</item>
/// <item>Every movement (receipt or issue) moves a valid <see cref="Quantity"/>: between 1 and <see cref="Quantity.Max"/> units.</item>
/// <item>Stock on hand can never exceed <see cref="MaxOnHand"/> units.</item>
/// <item>A movement that breaks a rule changes nothing.</item>
/// </list>
/// </summary>
public sealed class StockItem
{
    /// <summary>The largest number of units that can be on hand for one SKU.</summary>
    public const int MaxOnHand = int.MaxValue;

    public Sku Sku { get; }

    /// <summary>Units physically available. Never negative.</summary>
    public int OnHand { get; private set; }

    private StockItem(Sku sku, int onHand)
    {
        Sku = sku;
        OnHand = onHand;
    }

    /// <summary>A new item starts with nothing on hand.</summary>
    public static StockItem Create(Sku sku) => new(sku, 0);

    /// <summary>Rebuilds an item from storage. The same rules apply: on hand can never be negative.</summary>
    public static StockItem Restore(Sku sku, int onHand)
    {
        if (onHand < 0)
        {
            throw new DomainException($"Stock on hand of {sku} cannot be negative (was {onHand}).");
        }

        return new StockItem(sku, onHand);
    }

    /// <summary>Goods arrive: on hand grows by <paramref name="quantity"/>.</summary>
    public void Receive(Quantity quantity)
    {
        if (quantity.Value > MaxOnHand - OnHand)
        {
            throw new DomainException($"Receiving {quantity} of {Sku} would exceed the maximum of {MaxOnHand} on hand.");
        }

        OnHand += quantity.Value;
    }

    /// <summary>Goods leave: on hand shrinks by <paramref name="quantity"/>. Stock on hand can never go negative.</summary>
    public void Issue(Quantity quantity)
    {
        if (quantity.Value > OnHand)
        {
            throw new InsufficientStockException(Sku, quantity.Value, OnHand);
        }

        OnHand -= quantity.Value;
    }
}
