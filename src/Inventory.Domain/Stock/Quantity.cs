namespace Inventory.Domain.Stock;

/// <summary>
/// The number of units one receipt or issue moves: between 1 and <see cref="Max"/>.
/// </summary>
public readonly record struct Quantity
{
    /// <summary>The largest number of units one receipt or issue can move.</summary>
    public const int Max = 1_000_000;

    public int Value { get; }

    private Quantity(int value) => Value = value;

    /// <summary>Reads a quantity from user input. Throws <see cref="DomainException"/> when it is out of range.</summary>
    public static Quantity From(int value)
    {
        if (value is < 1 or > Max)
        {
            throw new DomainException($"Quantity must be between 1 and {Max} (was {value}).");
        }

        return new Quantity(value);
    }

    public override string ToString() => Value.ToString(System.Globalization.CultureInfo.InvariantCulture);
}
