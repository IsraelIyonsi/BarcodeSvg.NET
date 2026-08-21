using System.Text;
using BarcodeSvg;

namespace BarcodeSvg.Net.Tests.UpcA;

public sealed class UpcAEncoderTests
{
    // "03600029145" is the classic worked UPC-A example; its check digit is 2, giving "036000291452".
    private const string ExampleDataDigits = "03600029145";
    private const int ExampleCheckDigit = 2;
    private const string ExampleFullDigits = "036000291452";

    // UPC-A is structurally the EAN-13 of "0" + the 12 UPC-A digits. Every module in the symbol is
    // exactly 1 unit wide, so the bit string alone (1 = bar, 0 = space) fully pins the pattern.
    public static TheoryData<string> UpcAFullValues => new()
    {
        ExampleFullDigits,
        "012345678905",
        "123456789012",
        "042100005264",
        "614141000036",
    };

    [Fact]
    public void ComputeCheckDigit_KnownExample_ReturnsTwo()
    {
        Assert.Equal(ExampleCheckDigit, UpcAEncoder.ComputeCheckDigit(ExampleDataDigits));
    }

    [Fact]
    public void Encode_ElevenDigitInput_AppendsComputedCheckDigit()
    {
        var pattern = UpcAEncoder.Encode(ExampleDataDigits);

        Assert.Equal(ExampleFullDigits, pattern.HumanReadableText);
    }

    [Fact]
    public void Encode_TwelveDigitInputWithCorrectCheckDigit_IsAccepted()
    {
        var pattern = UpcAEncoder.Encode(ExampleFullDigits);

        Assert.Equal(ExampleFullDigits, pattern.HumanReadableText);
    }

    [Fact]
    public void Encode_TwelveDigitInputWithWrongCheckDigit_ThrowsFormatException()
    {
        // Correct check digit is 2; supplying 3 must be rejected.
        var exception = Assert.Throws<FormatException>(() => UpcAEncoder.Encode("036000291453"));

        Assert.Contains("check digit", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    // Proves reuse and correctness: the UPC-A bars are exactly the EAN-13 bars of "0" + the 12 digits.
    [Theory]
    [MemberData(nameof(UpcAFullValues))]
    public void Encode_BarsEqualEan13OfZeroPrefixedValue(string fullDigits)
    {
        var upcA = UpcAEncoder.Encode(fullDigits);
        var ean13 = Ean13Encoder.Encode(UpcASymbologyPrefix + fullDigits);

        Assert.Equal(ean13.Bars, upcA.Bars);
    }

    [Theory]
    [MemberData(nameof(UpcAFullValues))]
    public void Encode_ElevenDigitInputEqualsTwelveDigitInputPattern(string fullDigits)
    {
        var elevenDigits = fullDigits[..^1];

        var fromEleven = UpcAEncoder.Encode(elevenDigits);
        var fromTwelve = UpcAEncoder.Encode(fullDigits);

        Assert.Equal(BitsToString(fromTwelve), BitsToString(fromEleven));
    }

    [Theory]
    [MemberData(nameof(UpcAFullValues))]
    public void Encode_TotalWidthIsNinetyFiveModules(string fullDigits)
    {
        var pattern = UpcAEncoder.Encode(fullDigits);

        Assert.Equal(95, pattern.TotalModules);
    }

    // The human-readable text is the 12-digit UPC-A value, not the 13-digit EAN-13 value.
    [Theory]
    [MemberData(nameof(UpcAFullValues))]
    public void Encode_HumanReadableTextIsTheTwelveDigitValue(string fullDigits)
    {
        var pattern = UpcAEncoder.Encode(fullDigits);

        Assert.Equal(fullDigits, pattern.HumanReadableText);
    }

    [Fact]
    public void Encode_NullDigits_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => UpcAEncoder.Encode(null!));
    }

    [Theory]
    [InlineData("")]
    [InlineData("123")]
    [InlineData("0360002914")] // 10 digits
    [InlineData("0360002914521")] // 13 digits
    public void Encode_WrongLength_ThrowsArgumentException(string digits)
    {
        Assert.Throws<ArgumentException>(() => UpcAEncoder.Encode(digits));
    }

    [Theory]
    [InlineData("0360002914A")] // non-digit, 11 chars
    [InlineData("03600029145X")] // non-digit, 12 chars
    [InlineData("036000 91452")]
    public void Encode_NonDigitCharacter_ThrowsArgumentException(string digits)
    {
        Assert.Throws<ArgumentException>(() => UpcAEncoder.Encode(digits));
    }

    [Fact]
    public void ComputeCheckDigit_WrongLength_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => UpcAEncoder.ComputeCheckDigit("123"));
    }

    // The EAN-13 number-system prefix a UPC-A value carries implicitly.
    private const string UpcASymbologyPrefix = "0";

    // Bars is run-length-encoded, so each segment is expanded back to one bit per module.
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
