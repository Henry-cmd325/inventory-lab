using CsCheck;
using Inventory.Domain;
using Inventory.Domain.Stock;

namespace Inventory.Domain.Tests;

public class StockItemProperties
{
    private static readonly Sku AnySku = Sku.Parse("SKU-1");

    /// <summary>Quantities around every boundary: invalid, small, around MaxMovement.</summary>
    private static readonly Gen<int> Quantities = Gen.OneOf(
        Gen.Int[-3, 0],
        Gen.Int[1, 50],
        Gen.Int[Quantity.Max - 2, Quantity.Max + 2],
        Gen.Const(int.MinValue),
        Gen.Const(int.MaxValue));

    private enum Op { Receive, Issue }

    private static readonly Gen<(Op Op, int Quantity)> Operation = Gen.Select(Gen.Enum<Op>(), Quantities);

    [Fact]
    public void On_hand_never_goes_negative_and_matches_a_simple_model() =>
        Gen.Select(Gen.Int[0, 100], Operation.Array[0, 80]).Sample((start, ops) =>
        {
            var item = StockItem.Restore(AnySku, start);
            long model = start;

            foreach (var (op, quantity) in ops)
            {
                var validMovement = quantity is >= 1 and <= Quantity.Max;
                var expectedOk = op == Op.Receive
                    ? validMovement && model + quantity <= StockItem.MaxOnHand
                    : validMovement && quantity <= model;

                if (expectedOk)
                {
                    Apply(item, op, quantity);
                    model += op == Op.Receive ? quantity : -quantity;
                }
                else
                {
                    Assert.ThrowsAny<DomainException>(() => Apply(item, op, quantity));
                }

                Assert.True(item.OnHand >= 0);
                Assert.Equal(model, item.OnHand);
            }
        });

    [Fact]
    public void A_rejected_movement_changes_nothing() =>
        Gen.Select(Gen.Int[0, 1000], Operation).Sample((start, move) =>
        {
            var item = StockItem.Restore(AnySku, start);
            try
            {
                Apply(item, move.Op, move.Quantity);
            }
            catch (DomainException)
            {
                Assert.Equal(start, item.OnHand);
            }
        });

    [Fact]
    public void Issuing_more_than_on_hand_fails_with_insufficient_stock() =>
        Gen.Select(Gen.Int[0, 1000], Gen.Int[1, 1000]).Where((onHand, extra) => onHand + extra <= Quantity.Max)
            .Sample((onHand, extra) =>
            {
                var item = StockItem.Restore(AnySku, onHand);
                var error = Assert.Throws<InsufficientStockException>(() => item.Issue(Quantity.From(onHand + extra)));
                Assert.Equal(onHand + extra, error.Requested);
                Assert.Equal(onHand, error.OnHand);
                Assert.Equal(AnySku, error.Sku);
                Assert.Equal($"Cannot issue {onHand + extra} of SKU-1: only {onHand} on hand.", error.Message);
            });

    [Fact]
    public void Issuing_everything_on_hand_leaves_zero() =>
        Gen.Int[1, Quantity.Max].Sample(onHand =>
        {
            var item = StockItem.Restore(AnySku, onHand);
            item.Issue(Quantity.From(onHand));
            Assert.Equal(0, item.OnHand);
        });

    [Fact]
    public void Receiving_then_issuing_the_same_quantity_returns_to_the_start() =>
        Gen.Select(Gen.Int[0, 10_000], Gen.Int[1, Quantity.Max]).Sample((start, quantity) =>
        {
            var item = StockItem.Restore(AnySku, start);
            item.Receive(Quantity.From(quantity));
            Assert.Equal(start + quantity, item.OnHand);
            item.Issue(Quantity.From(quantity));
            Assert.Equal(start, item.OnHand);
        });

    [Fact]
    public void Receiving_up_to_the_maximum_on_hand_is_allowed_and_one_more_is_not() =>
        Gen.Int[1, Quantity.Max].Sample(quantity =>
        {
            var full = StockItem.Restore(AnySku, StockItem.MaxOnHand - quantity);
            full.Receive(Quantity.From(quantity));
            Assert.Equal(StockItem.MaxOnHand, full.OnHand);

            var almost = StockItem.Restore(AnySku, StockItem.MaxOnHand - quantity + 1);
            var error = Assert.Throws<DomainException>(() => almost.Receive(Quantity.From(quantity)));
            Assert.Contains("would exceed the maximum", error.Message, StringComparison.Ordinal);
            Assert.Equal(StockItem.MaxOnHand - quantity + 1, almost.OnHand);
        });

    [Fact]
    public void Restore_rejects_negative_on_hand_and_keeps_valid_values() =>
        Gen.Int.Sample(onHand =>
        {
            if (onHand < 0)
            {
                var error = Assert.Throws<DomainException>(() => StockItem.Restore(AnySku, onHand));
                Assert.Contains($"(was {onHand})", error.Message, StringComparison.Ordinal);
            }
            else
            {
                var item = StockItem.Restore(AnySku, onHand);
                Assert.Equal(onHand, item.OnHand);
                Assert.Equal(AnySku, item.Sku);
            }
        });

    [Fact]
    public void A_new_item_starts_empty() =>
        Assert.Equal(0, StockItem.Create(AnySku).OnHand);

    [Theory]
    [InlineData(0)]
    [InlineData(Quantity.Max + 1)]
    public void Movement_limits_apply_to_receipts_and_issues(int quantity)
    {
        var item = StockItem.Restore(AnySku, Quantity.Max + 10);
        Assert.Throws<DomainException>(() => item.Receive(Quantity.From(quantity)));
        Assert.Throws<DomainException>(() => item.Issue(Quantity.From(quantity)));
    }

    [Fact]
    public void The_largest_movement_is_allowed()
    {
        var item = StockItem.Create(AnySku);
        item.Receive(Quantity.From(Quantity.Max));
        item.Issue(Quantity.From(Quantity.Max));
        Assert.Equal(0, item.OnHand);
    }

    private static void Apply(StockItem item, Op op, int quantity)
    {
        if (op == Op.Receive)
        {
            item.Receive(Quantity.From(quantity));
        }
        else
        {
            item.Issue(Quantity.From(quantity));
        }
    }
}
