namespace BarcodeSvg;

/// <summary>
/// One-call convenience wrapper over <see cref="UpcAEncoder"/> and <see cref="BarcodeSvgRenderer"/>.
/// </summary>
public static class UpcABarcode
{
    /// <summary>Encodes <paramref name="digits"/> as a UPC-A bar/space pattern.</summary>
    /// <param name="digits">11 or 12 ASCII digits. See <see cref="UpcAEncoder.Encode(string)"/> for constraints.</param>
    /// <returns>The resulting bar/space pattern.</returns>
    public static BarcodePattern Encode(string digits) => UpcAEncoder.Encode(digits);

    /// <summary>Encodes <paramref name="digits"/> as a UPC-A symbol and renders it directly to SVG.</summary>
    /// <param name="digits">11 or 12 ASCII digits. See <see cref="UpcAEncoder.Encode(string)"/> for constraints.</param>
    /// <param name="options">Sizing, color and text options. When <see langword="null"/>, default options are used.</param>
    /// <returns>A well-formed, self-contained SVG document string.</returns>
    public static string ToSvg(string digits, BarcodeRenderOptions? options = null) =>
        BarcodeSvgRenderer.Render(UpcAEncoder.Encode(digits), options);
}
