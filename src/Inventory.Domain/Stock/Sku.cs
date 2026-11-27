using System.Text.RegularExpressions;

namespace Inventory.Domain.Stock;

/// <summary>
/// Stock keeping unit: the code that identifies an item in the warehouse.
/// A SKU has 1 to 32 characters: upper-case letters A-Z, digits and dashes.
/// Input is trimmed and upper-cased first, so " ab-1 " and "AB-1" are the same SKU.
/// </summary>
public readonly partial record struct Sku
{
    public const int MaxLength = 32;

    public string Value { get; }

    private Sku(string value) => Value = value;

    /// <summary>Reads a SKU from user input. Throws <see cref="DomainException"/> when the format is wrong.</summary>
    public static Sku Parse(string? input)
    {
        var normalized = input?.Trim().ToUpperInvariant();
        if (normalized is null || !Format().IsMatch(normalized))
        {
            throw new DomainException($"SKU must have 1 to {MaxLength} characters: letters A-Z, digits or dashes.");
        }

        return new Sku(normalized);
    }

    public override string ToString() => Value;

    [GeneratedRegex("^[A-Z0-9-]{1,32}$")]
    private static partial Regex Format();
}
