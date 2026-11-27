using CsCheck;
using Inventory.Domain;
using Inventory.Domain.Stock;

namespace Inventory.Domain.Tests;

public class SkuProperties
{
    private static readonly Gen<char> ValidChar = Gen.OneOf(Gen.Char['A', 'Z'], Gen.Char['0', '9'], Gen.Const('-'));
    private static readonly Gen<string> ValidSku = ValidChar.Array[1, Sku.MaxLength].Select(cs => new string(cs));

    [Fact]
    public void Valid_codes_are_accepted_and_kept_as_written() =>
        ValidSku.Sample(code => Assert.Equal(code, Sku.Parse(code).Value));

    [Fact]
    public void Input_is_trimmed_and_upper_cased() =>
        ValidSku.Select(Gen.String[Gen.Char[" \t"], 0, 3], Gen.String[Gen.Char[" \t"], 0, 3]).Sample((code, left, right) =>
            Assert.Equal(code, Sku.Parse(left + code.ToLowerInvariant() + right).Value));

    [Fact]
    public void Parse_round_trips_through_ToString() =>
        ValidSku.Sample(code =>
        {
            var sku = Sku.Parse(code);
            Assert.Equal(sku, Sku.Parse(sku.ToString()));
        });

    [Fact]
    public void Codes_longer_than_the_maximum_are_rejected() =>
        ValidChar.Array[Sku.MaxLength + 1, Sku.MaxLength + 20].Sample(cs =>
            Assert.Throws<DomainException>(() => Sku.Parse(new string(cs))));

    [Fact]
    public void Codes_with_a_forbidden_character_are_rejected() =>
        Gen.Select(ValidSku, Gen.Char.Where(c => !char.IsAsciiLetterOrDigit(c) && c != '-' && !char.IsWhiteSpace(c)), Gen.Int)
            .Sample((code, bad, at) =>
            {
                var position = Math.Abs(at % (code.Length + 1));
                Assert.Throws<DomainException>(() => Sku.Parse(code.Insert(position, bad.ToString())));
            });

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Empty_codes_are_rejected(string? input) =>
        Assert.Throws<DomainException>(() => Sku.Parse(input));

    [Fact]
    public void The_error_explains_the_format() =>
        Assert.Contains("1 to 32 characters", Assert.Throws<DomainException>(() => Sku.Parse("bad_sku")).Message, StringComparison.Ordinal);

    [Fact]
    public void Inner_whitespace_is_rejected() =>
        Assert.Throws<DomainException>(() => Sku.Parse("AB 1"));
}
