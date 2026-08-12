namespace BarcodeSvg;

/// <summary>
/// One-call convenience wrapper over <see cref="Ean13Encoder"/> and <see cref="BarcodeSvgRenderer"/>.
/// </summary>
public static class Ean13Barcode
{
    /// <summary>Encodes <paramref name="digits"/> as an EAN-13 bar/space pattern.</summary>
    /// <param name="digits">12 or 13 ASCII digits. See <see cref="Ean13Encoder.Encode(string)"/> for constraints.</param>
    /// <returns>The resulting bar/space pattern.</returns>
    public static BarcodePattern Encode(string digits) => Ean13Encoder.Encode(digits);

    /// <summary>Encodes <paramref name="digits"/> as an EAN-13 symbol and renders it directly to SVG.</summary>
    /// <param name="digits">12 or 13 ASCII digits. See <see cref="Ean13Encoder.Encode(string)"/> for constraints.</param>
    /// <param name="options">Sizing, color and text options. When <see langword="null"/>, default options are used.</param>
    /// <returns>A well-formed, self-contained SVG document string.</returns>
    public static string ToSvg(string digits, BarcodeRenderOptions? options = null) =>
        BarcodeSvgRenderer.Render(Ean13Encoder.Encode(digits), options);
}
