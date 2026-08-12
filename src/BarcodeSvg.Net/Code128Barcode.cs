namespace BarcodeSvg;

/// <summary>
/// One-call convenience wrapper over <see cref="Code128Encoder"/> and <see cref="BarcodeSvgRenderer"/>.
/// </summary>
public static class Code128Barcode
{
    /// <summary>Encodes <paramref name="data"/> as a Code 128 bar/space pattern.</summary>
    /// <param name="data">The text to encode. See <see cref="Code128Encoder.Encode(string)"/> for constraints.</param>
    /// <returns>The resulting bar/space pattern.</returns>
    public static BarcodePattern Encode(string data) => Code128Encoder.Encode(data);

    /// <summary>Encodes <paramref name="data"/> as a Code 128 symbol and renders it directly to SVG.</summary>
    /// <param name="data">The text to encode. See <see cref="Code128Encoder.Encode(string)"/> for constraints.</param>
    /// <param name="options">Sizing, color and text options. When <see langword="null"/>, default options are used.</param>
    /// <returns>A well-formed, self-contained SVG document string.</returns>
    public static string ToSvg(string data, BarcodeRenderOptions? options = null) =>
        BarcodeSvgRenderer.Render(Code128Encoder.Encode(data), options);
}
