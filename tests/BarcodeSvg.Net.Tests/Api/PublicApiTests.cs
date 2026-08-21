using BarcodeSvg;

namespace BarcodeSvg.Net.Tests.Api;

public sealed class PublicApiTests
{
    [Fact]
    public void Code128Barcode_Encode_MatchesCode128Encoder()
    {
        var viaFacade = Code128Barcode.Encode("PJJ123C");
        var viaEncoder = Code128Encoder.Encode("PJJ123C");

        Assert.Equal(viaEncoder.Bars, viaFacade.Bars);
    }

    [Fact]
    public void Code128Barcode_ToSvg_MatchesManualEncodeThenRender()
    {
        var options = new BarcodeRenderOptions { ModuleWidth = 3 };

        var viaFacade = Code128Barcode.ToSvg("PJJ123C", options);
        var viaManualCall = BarcodeSvgRenderer.Render(Code128Encoder.Encode("PJJ123C"), options);

        Assert.Equal(viaManualCall, viaFacade);
    }

    [Fact]
    public void Ean13Barcode_Encode_MatchesEan13Encoder()
    {
        var viaFacade = Ean13Barcode.Encode("4006381333931");
        var viaEncoder = Ean13Encoder.Encode("4006381333931");

        Assert.Equal(viaEncoder.Bars, viaFacade.Bars);
    }

    [Fact]
    public void Ean13Barcode_ToSvg_MatchesManualEncodeThenRender()
    {
        var options = new BarcodeRenderOptions { ShowText = false };

        var viaFacade = Ean13Barcode.ToSvg("4006381333931", options);
        var viaManualCall = BarcodeSvgRenderer.Render(Ean13Encoder.Encode("4006381333931"), options);

        Assert.Equal(viaManualCall, viaFacade);
    }

    [Fact]
    public void UpcABarcode_Encode_MatchesUpcAEncoder()
    {
        var viaFacade = UpcABarcode.Encode("036000291452");
        var viaEncoder = UpcAEncoder.Encode("036000291452");

        Assert.Equal(viaEncoder.Bars, viaFacade.Bars);
    }

    [Fact]
    public void UpcABarcode_ToSvg_MatchesManualEncodeThenRender()
    {
        var options = new BarcodeRenderOptions { ModuleWidth = 3 };

        var viaFacade = UpcABarcode.ToSvg("036000291452", options);
        var viaManualCall = BarcodeSvgRenderer.Render(UpcAEncoder.Encode("036000291452"), options);

        Assert.Equal(viaManualCall, viaFacade);
    }

    // With the human-readable line hidden, a UPC-A SVG is byte-identical to the EAN-13 SVG of
    // "0" + the 12 digits, proving UPC-A reuses the EAN-13 encoding and the shared SVG renderer.
    [Fact]
    public void UpcABarcode_ToSvg_WithTextHidden_EqualsEan13SvgOfZeroPrefixedValue()
    {
        var options = new BarcodeRenderOptions { ShowText = false };

        var upcASvg = UpcABarcode.ToSvg("036000291452", options);
        var ean13Svg = Ean13Barcode.ToSvg("0036000291452", options);

        Assert.Equal(ean13Svg, upcASvg);
    }

    [Fact]
    public void UpcABarcode_ToSvg_RenderOptionsFlowThrough()
    {
        var narrow = UpcABarcode.ToSvg("036000291452", new BarcodeRenderOptions { ModuleWidth = 2 });
        var wide = UpcABarcode.ToSvg("036000291452", new BarcodeRenderOptions { ModuleWidth = 4 });

        Assert.NotEqual(narrow, wide);
    }

    [Fact]
    public void BarcodePattern_TotalModules_IsSumOfSegmentWidths()
    {
        var pattern = Code128Encoder.Encode("1234567890");

        var expectedTotal = pattern.Bars.Sum(b => b.WidthInModules);

        Assert.Equal(expectedTotal, pattern.TotalModules);
    }

    [Fact]
    public void BarcodePattern_NullBars_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new BarcodePattern(null!, 0, "text"));
    }

    [Fact]
    public void BarcodePattern_EmptyBars_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => new BarcodePattern(Array.Empty<BarSegment>(), 0, "text"));
    }

    [Fact]
    public void BarcodePattern_NegativeQuietZone_ThrowsArgumentOutOfRangeException()
    {
        var bars = new[] { new BarSegment(true, 1) };

        Assert.Throws<ArgumentOutOfRangeException>(() => new BarcodePattern(bars, -1, "text"));
    }

    [Fact]
    public void BarcodePattern_ZeroWidthSegment_ThrowsArgumentException()
    {
        var bars = new[] { new BarSegment(true, 0) };

        Assert.Throws<ArgumentException>(() => new BarcodePattern(bars, 0, "text"));
    }

    [Fact]
    public void BarSegment_Equality_IsValueBased()
    {
        Assert.Equal(new BarSegment(true, 3), new BarSegment(true, 3));
        Assert.NotEqual(new BarSegment(true, 3), new BarSegment(false, 3));
        Assert.NotEqual(new BarSegment(true, 3), new BarSegment(true, 4));
    }
}
