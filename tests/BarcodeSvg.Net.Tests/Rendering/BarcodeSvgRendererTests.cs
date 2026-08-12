using System.Globalization;
using System.Xml.Linq;
using BarcodeSvg;

namespace BarcodeSvg.Net.Tests.Rendering;

public sealed class BarcodeSvgRendererTests
{
    private static readonly BarcodePattern Ean13Pattern = Ean13Encoder.Encode("4006381333931");
    private static readonly BarcodePattern Code128Pattern = Code128Encoder.Encode("PJJ123C");

    [Theory]
    [MemberData(nameof(Patterns))]
    public void Render_ProducesWellFormedXml(BarcodePattern pattern)
    {
        var svg = BarcodeSvgRenderer.Render(pattern);

        var document = XDocument.Parse(svg);

        Assert.Equal("svg", document.Root!.Name.LocalName);
    }

    [Theory]
    [MemberData(nameof(Patterns))]
    public void Render_SameInputAndOptions_IsByteForByteDeterministic(BarcodePattern pattern)
    {
        var options = new BarcodeRenderOptions { BarColor = "#123456", ModuleWidth = 3.5 };

        var first = BarcodeSvgRenderer.Render(pattern, options);
        var second = BarcodeSvgRenderer.Render(pattern, options);

        Assert.Equal(first, second);
    }

    [Theory]
    [InlineData("<script>alert(1)</script>")]
    [InlineData("\"><img src=x onerror=alert(1)>")]
    [InlineData("Tom & Jerry's \"Barcode\" <Test>")]
    public void Render_MaliciousHumanReadableText_IsEscapedAndDoesNotBreakXml(string maliciousText)
    {
        var options = new BarcodeRenderOptions { Text = maliciousText };

        var svg = BarcodeSvgRenderer.Render(Code128Pattern, options);
        var document = XDocument.Parse(svg);

        var textElement = document.Root!.Elements().Single(e => e.Name.LocalName == "text");
        Assert.Equal(maliciousText, textElement.Value);
        Assert.DoesNotContain("<script>", svg, StringComparison.Ordinal);
        Assert.DoesNotContain("<img", svg, StringComparison.Ordinal);
    }

    // XML 1.0 has no legal representation, escaped or otherwise, for most ASCII control
    // characters (0x00-0x08, 0x0B, 0x0C, 0x0E-0x1F). Code 128 explicitly supports encoding them
    // (Code Set A), ShowText defaults to true, and the default text is the raw data, so this data
    // must flow all the way to a well-formed <text> element without XmlException, and without
    // silently reproducing the illegal byte verbatim in the output string.
    [Fact]
    public void Render_ControlCharacterData_DefaultTextPath_ProducesWellFormedXml()
    {
        var pattern = Code128Encoder.Encode("\u0001ABC");

        var svg = BarcodeSvgRenderer.Render(pattern);
        var document = XDocument.Parse(svg);

        var textElement = document.Root!.Elements().Single(e => e.Name.LocalName == "text");
        Assert.DoesNotContain('\u0001', textElement.Value);
        Assert.Contains('\uFFFD', textElement.Value);
        Assert.DoesNotContain("\u0001", svg, StringComparison.Ordinal);
    }

    [Fact]
    public void Render_MaliciousColorValues_AreEscapedAndDoNotBreakXml()
    {
        var options = new BarcodeRenderOptions { BarColor = "red\" onmouseover=\"alert(1)", BackgroundColor = "white\"/><script>x</script>" };

        var svg = BarcodeSvgRenderer.Render(Code128Pattern, options);
        var document = XDocument.Parse(svg);

        Assert.DoesNotContain("<script>", svg, StringComparison.Ordinal);
        Assert.NotNull(document.Root);
    }

    [Fact]
    public void Render_DefaultOptions_ComputesExpectedGeometry()
    {
        var svg = BarcodeSvgRenderer.Render(Code128Pattern);
        var document = XDocument.Parse(svg);

        var expectedBarsWidth = Code128Pattern.TotalModules * BarcodeRenderOptions.DefaultModuleWidth;
        var expectedQuietZone = Code128Pattern.RecommendedQuietZoneModules * BarcodeRenderOptions.DefaultModuleWidth;
        var expectedWidth = expectedBarsWidth + (2 * expectedQuietZone);
        var expectedHeight = BarcodeRenderOptions.DefaultHeight + BarcodeRenderOptions.DefaultFontSize + BarcodeRenderOptions.DefaultTextMargin;

        Assert.Equal(expectedWidth, ParseDouble(document.Root!.Attribute("width")!.Value), 3);
        Assert.Equal(expectedHeight, ParseDouble(document.Root!.Attribute("height")!.Value), 3);
    }

    [Fact]
    public void Render_ExplicitModuleWidth_ScalesBarsAccordingly()
    {
        var svg = BarcodeSvgRenderer.Render(Code128Pattern, new BarcodeRenderOptions { ModuleWidth = 5, ShowText = false });
        var document = XDocument.Parse(svg);

        var expectedWidth = (Code128Pattern.TotalModules * 5.0) + (2 * Code128Pattern.RecommendedQuietZoneModules * 5.0);
        Assert.Equal(expectedWidth, ParseDouble(document.Root!.Attribute("width")!.Value), 3);
    }

    [Fact]
    public void Render_ExplicitWidth_DerivesModuleWidth()
    {
        var svg = BarcodeSvgRenderer.Render(Code128Pattern, new BarcodeRenderOptions { Width = 220, ShowText = false });
        var document = XDocument.Parse(svg);

        var quietZoneModuleWidth = 220.0 / Code128Pattern.TotalModules;
        var expectedWidth = 220 + (2 * Code128Pattern.RecommendedQuietZoneModules * quietZoneModuleWidth);
        Assert.Equal(expectedWidth, ParseDouble(document.Root!.Attribute("width")!.Value), 3);
    }

    [Fact]
    public void Render_ShowTextFalse_OmitsTextElement()
    {
        var svg = BarcodeSvgRenderer.Render(Code128Pattern, new BarcodeRenderOptions { ShowText = false });
        var document = XDocument.Parse(svg);

        Assert.DoesNotContain(document.Root!.Elements(), e => e.Name.LocalName == "text");
    }

    [Fact]
    public void Render_ShowTextTrue_TextElementDefaultsToHumanReadableText()
    {
        var svg = BarcodeSvgRenderer.Render(Code128Pattern);
        var document = XDocument.Parse(svg);

        var textElement = document.Root!.Elements().Single(e => e.Name.LocalName == "text");
        Assert.Equal(Code128Pattern.HumanReadableText, textElement.Value);
    }

    [Fact]
    public void Render_RectCountMatchesBarSegments()
    {
        var svg = BarcodeSvgRenderer.Render(Ean13Pattern, new BarcodeRenderOptions { ShowText = false });
        var document = XDocument.Parse(svg);

        var expectedBarCount = Ean13Pattern.Bars.Count(b => b.IsBar);
        var rectCount = document.Root!.Elements().Count(e => e.Name.LocalName == "rect");

        Assert.Equal(expectedBarCount, rectCount);
    }

    [Fact]
    public void Render_BackgroundColorSet_AddsLeadingBackgroundRect()
    {
        var svg = BarcodeSvgRenderer.Render(Ean13Pattern, new BarcodeRenderOptions { BackgroundColor = "#ffffff", ShowText = false });
        var document = XDocument.Parse(svg);

        var expectedBarCount = Ean13Pattern.Bars.Count(b => b.IsBar);
        var rectCount = document.Root!.Elements().Count(e => e.Name.LocalName == "rect");

        Assert.Equal(expectedBarCount + 1, rectCount);
    }

    [Fact]
    public void Render_NullPattern_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => BarcodeSvgRenderer.Render(null!));
    }

    [Fact]
    public void Render_WidthAndModuleWidthBothSet_ThrowsArgumentException()
    {
        var options = new BarcodeRenderOptions { Width = 100, ModuleWidth = 2 };

        Assert.Throws<ArgumentException>(() => BarcodeSvgRenderer.Render(Code128Pattern, options));
    }

    [Theory]
    [InlineData(0.0)]
    [InlineData(-1.0)]
    public void Render_NonPositiveModuleWidth_ThrowsArgumentException(double moduleWidth)
    {
        var options = new BarcodeRenderOptions { ModuleWidth = moduleWidth };

        Assert.Throws<ArgumentException>(() => BarcodeSvgRenderer.Render(Code128Pattern, options));
    }

    [Theory]
    [InlineData(0.0)]
    [InlineData(-5.0)]
    public void Render_NonPositiveHeight_ThrowsArgumentException(double height)
    {
        var options = new BarcodeRenderOptions { Height = height };

        Assert.Throws<ArgumentException>(() => BarcodeSvgRenderer.Render(Code128Pattern, options));
    }

    [Fact]
    public void Render_NegativeQuietZoneWidth_ThrowsArgumentException()
    {
        var options = new BarcodeRenderOptions { QuietZoneWidth = -1 };

        Assert.Throws<ArgumentException>(() => BarcodeSvgRenderer.Render(Code128Pattern, options));
    }

    [Fact]
    public void Render_EmptyBarColor_ThrowsArgumentException()
    {
        var options = new BarcodeRenderOptions { BarColor = string.Empty };

        Assert.Throws<ArgumentException>(() => BarcodeSvgRenderer.Render(Code128Pattern, options));
    }

    public static TheoryData<BarcodePattern> Patterns => new()
    {
        Ean13Pattern,
        Code128Pattern,
    };

    private static double ParseDouble(string value) => double.Parse(value, CultureInfo.InvariantCulture);
}
