using System.Text;
using BarcodeSvg;

namespace BarcodeSvg.Net.Tests.Ean13;

public sealed class Ean13EncoderTests
{
    // Expected bit patterns below were computed by an independently written Python reference
    // (not derived from this library) that assembles the guard patterns, the L/G/R digit tables
    // and the leading-digit parity table exactly as published, and separately re-derives the
    // weighted modulo-10 check digit. Every module in EAN-13 is exactly 1 unit wide, so the bit
    // string alone (1 = bar, 0 = space) fully pins the pattern. "4006381333931" is the classic
    // worked EAN-13 example (a 375g box of Kellogg's Corn Flakes) used throughout the literature.
    public static TheoryData<string, string> ExactPatternFixtures => new()
    {
        {
            "4006381333931",
            "10100011010100111010111101111010001001011001101010100001010000101000010111010010000101100110101"
        },
        {
            // Same value supplied as 12 digits; the check digit (1) is computed and appended.
            "400638133393",
            "10100011010100111010111101111010001001011001101010100001010000101000010111010010000101100110101"
        },
        {
            "0123456789050",
            "10100110010010011011110101000110110001010111101010100010010010001110100111001010011101110010101"
        },
        {
            "5901234123457",
            "10100010110100111011001100100110111101001110101010110011011011001000010101110010011101000100101"
        },
        {
            "1234567890128",
            "10100100110111101001110101100010000101001000101010100100011101001110010110011011011001001000101"
        },

        // One fixture per leading digit, pinning the parity pattern (L/G sequence) that digit selects.
        { "0000000000000", "10100011010001101000110100011010001101000110101010111001011100101110010111001011100101110010101" },
        { "1000000000009", "10100011010001101010011100011010100111010011101010111001011100101110010111001011100101110100101" },
        { "2000000000008", "10100011010001101010011101001110001101010011101010111001011100101110010111001011100101001000101" },
        { "3000000000007", "10100011010001101010011101001110100111000110101010111001011100101110010111001011100101000100101" },
        { "4000000000006", "10100011010100111000110100011010100111010011101010111001011100101110010111001011100101010000101" },
        { "5000000000005", "10100011010100111010011100011010001101010011101010111001011100101110010111001011100101001110101" },
        { "6000000000004", "10100011010100111010011101001110001101000110101010111001011100101110010111001011100101011100101" },
        { "7000000000003", "10100011010100111000110101001110001101010011101010111001011100101110010111001011100101000010101" },
        { "8000000000002", "10100011010100111000110101001110100111000110101010111001011100101110010111001011100101101100101" },
        { "9000000000001", "10100011010100111010011100011010100111000110101010111001011100101110010111001011100101100110101" },
    };

    [Theory]
    [MemberData(nameof(ExactPatternFixtures))]
    public void Encode_ProducesExactModulePattern(string input, string expectedBits)
    {
        var pattern = Ean13Encoder.Encode(input);

        Assert.Equal(expectedBits, BitsToString(pattern));
    }

    [Theory]
    [MemberData(nameof(ExactPatternFixtures))]
    public void Encode_TotalWidthIsNinetyFiveModules(string input, string _)
    {
        var pattern = Ean13Encoder.Encode(input);

        Assert.Equal(95, pattern.TotalModules);
    }

    // BarcodePattern documents that Bars is a run-length-encoded, alternating-polarity sequence
    // (see BarcodePattern's constructor doc), not one entry per module: adjacent same-polarity
    // modules across digit and guard boundaries are merged into a single wider segment, so
    // Bars.Count is normally less than the 95-module total width, never equal to it.
    [Theory]
    [MemberData(nameof(ExactPatternFixtures))]
    public void Encode_BarsAlwaysStartOnABarAndStrictlyAlternate(string input, string _)
    {
        var pattern = Ean13Encoder.Encode(input);

        Assert.True(pattern.Bars[0].IsBar);
        for (var i = 1; i < pattern.Bars.Count; i++)
        {
            Assert.NotEqual(pattern.Bars[i - 1].IsBar, pattern.Bars[i].IsBar);
        }
    }

    [Theory]
    [MemberData(nameof(ExactPatternFixtures))]
    public void Encode_ReportsEan13RecommendedQuietZone(string input, string _)
    {
        var pattern = Ean13Encoder.Encode(input);

        Assert.Equal(9, pattern.RecommendedQuietZoneModules);
    }

    [Theory]
    [InlineData("400638133393", 1)]
    [InlineData("000000000000", 0)]
    [InlineData("999999999998", 7)]
    [InlineData("123456789012", 8)]
    [InlineData("590123412345", 7)]
    public void ComputeCheckDigit_MatchesWeightedModulo10Formula(string twelveDigits, int expectedCheckDigit)
    {
        Assert.Equal(expectedCheckDigit, Ean13Encoder.ComputeCheckDigit(twelveDigits));
    }

    [Fact]
    public void Encode_ThirteenDigitInput_HumanReadableTextIsTheFullValue()
    {
        var pattern = Ean13Encoder.Encode("4006381333931");

        Assert.Equal("4006381333931", pattern.HumanReadableText);
    }

    [Fact]
    public void Encode_TwelveDigitInput_AppendsComputedCheckDigit()
    {
        var pattern = Ean13Encoder.Encode("400638133393");

        Assert.Equal("4006381333931", pattern.HumanReadableText);
    }

    [Fact]
    public void Encode_ThirteenDigitInputWithWrongCheckDigit_ThrowsFormatException()
    {
        var exception = Assert.Throws<FormatException>(() => Ean13Encoder.Encode("4006381333930"));

        Assert.Contains("check digit", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Encode_NullDigits_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => Ean13Encoder.Encode(null!));
    }

    [Theory]
    [InlineData("")]
    [InlineData("123")]
    [InlineData("12345678901")]
    [InlineData("12345678901234")]
    public void Encode_WrongLength_ThrowsArgumentException(string digits)
    {
        Assert.Throws<ArgumentException>(() => Ean13Encoder.Encode(digits));
    }

    [Theory]
    [InlineData("40063813339A")]
    [InlineData("4006381333 3")]
    [InlineData("400638133-393")]
    public void Encode_NonDigitCharacter_ThrowsArgumentException(string digits)
    {
        Assert.Throws<ArgumentException>(() => Ean13Encoder.Encode(digits));
    }

    [Fact]
    public void ComputeCheckDigit_WrongLength_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => Ean13Encoder.ComputeCheckDigit("123"));
    }

    // Bars is run-length-encoded (see Encode_BarsAlwaysStartOnABarAndStrictlyAlternate), so each
    // segment is expanded back to one bit per module to reconstruct the flat fixture string.
    private static string BitsToString(BarcodePattern pattern)
    {
        var bits = new StringBuilder(pattern.TotalModules);
        foreach (var bar in pattern.Bars)
        {
            bits.Append(bar.IsBar ? '1' : '0', bar.WidthInModules);
        }

        return bits.ToString();
    }
}
