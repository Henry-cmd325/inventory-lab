using CsCheck;
using Inventory.Domain;
using Inventory.Domain.Stock;

namespace Inventory.Domain.Tests;

public class QuantityProperties
{
    [Fact]
    public void Values_from_1_to_the_maximum_are_accepted_and_kept() =>
        Gen.Int[1, Quantity.Max].Sample(value => Assert.Equal(value, Quantity.From(value).Value));

    [Fact]
    public void Values_below_1_are_rejected() =>
        Gen.Int[int.MinValue, 0].Sample(value => Assert.Throws<DomainException>(() => Quantity.From(value)));

    [Fact]
    public void Values_above_the_maximum_are_rejected() =>
        Gen.Int[Quantity.Max + 1, int.MaxValue].Sample(value => Assert.Throws<DomainException>(() => Quantity.From(value)));

    [Theory]
    [InlineData(1)]
    [InlineData(Quantity.Max)]
    public void Boundaries_are_inside(int value) => Assert.Equal(value, Quantity.From(value).Value);

    [Theory]
    [InlineData(0)]
    [InlineData(Quantity.Max + 1)]
    public void Just_outside_the_boundaries_is_rejected(int value) =>
        Assert.Throws<DomainException>(() => Quantity.From(value));

    [Fact]
    public void The_error_names_the_rejected_value() =>
        Gen.OneOf(Gen.Int[int.MinValue, 0], Gen.Int[Quantity.Max + 1, int.MaxValue]).Sample(value =>
        {
            var error = Assert.Throws<DomainException>(() => Quantity.From(value));
            Assert.Contains($"(was {value})", error.Message, StringComparison.Ordinal);
        });

    [Fact]
    public void ToString_is_the_plain_number() =>
        Gen.Int[1, Quantity.Max].Sample(value => Assert.Equal(value.ToString(System.Globalization.CultureInfo.InvariantCulture), Quantity.From(value).ToString()));
}
