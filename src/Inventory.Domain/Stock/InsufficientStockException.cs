namespace Inventory.Domain.Stock;

/// <summary>An issue asked for more units than are on hand. The API answers 409.</summary>
public sealed class InsufficientStockException(Sku sku, int requested, int onHand)
    : DomainException($"Cannot issue {requested} of {sku}: only {onHand} on hand.")
{
    public Sku Sku { get; } = sku;
    public int Requested { get; } = requested;
    public int OnHand { get; } = onHand;
}
