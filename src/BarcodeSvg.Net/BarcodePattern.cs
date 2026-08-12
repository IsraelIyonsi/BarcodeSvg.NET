namespace BarcodeSvg;

/// <summary>
/// The symbology-agnostic result of encoding a value into a 1D barcode: an ordered sequence of
/// bar and space runs (in modules), together with the metadata an SVG renderer needs.
/// </summary>
/// <remarks>
/// This is the "raw module or bar pattern" callers can use to render the barcode themselves
/// (a custom rasterizer, a different vector format, or a physical printer driver) instead of
/// using <see cref="BarcodeSvgRenderer"/>.
/// </remarks>
public sealed class BarcodePattern
{
    /// <summary>
    /// Initializes a new <see cref="BarcodePattern"/>.
    /// </summary>
    /// <param name="bars">The ordered bar and space runs, alternating polarity, starting with a bar.</param>
    /// <param name="recommendedQuietZoneModules">
    /// The symbology's recommended quiet zone width, in modules, applied on each side of the
    /// symbol when a renderer is not given an explicit override.
    /// </param>
    /// <param name="humanReadableText">The default human-readable text line for this symbol.</param>
    public BarcodePattern(IReadOnlyList<BarSegment> bars, int recommendedQuietZoneModules, string humanReadableText)
    {
        ArgumentNullException.ThrowIfNull(bars);
        ArgumentNullException.ThrowIfNull(humanReadableText);
        if (bars.Count == 0)
        {
            throw new ArgumentException("A barcode pattern must contain at least one bar or space segment.", nameof(bars));
        }

        if (recommendedQuietZoneModules < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(recommendedQuietZoneModules), recommendedQuietZoneModules, "Quiet zone width cannot be negative.");
        }

        Bars = bars;
        RecommendedQuietZoneModules = recommendedQuietZoneModules;
        HumanReadableText = humanReadableText;

        var totalModules = 0;
        foreach (var bar in bars)
        {
            if (bar.WidthInModules < 1)
            {
                throw new ArgumentException("Every bar or space segment must be at least 1 module wide.", nameof(bars));
            }

            totalModules += bar.WidthInModules;
        }

        TotalModules = totalModules;
    }

    /// <summary>The ordered bar and space runs that make up the symbol, in modules.</summary>
    public IReadOnlyList<BarSegment> Bars { get; }

    /// <summary>The total width of <see cref="Bars"/>, in modules.</summary>
    public int TotalModules { get; }

    /// <summary>
    /// The symbology's recommended quiet zone width, in modules, applied on each side of the
    /// symbol by <see cref="BarcodeSvgRenderer"/> when <see cref="BarcodeRenderOptions.QuietZoneWidth"/>
    /// is not set.
    /// </summary>
    public int RecommendedQuietZoneModules { get; }

    /// <summary>
    /// The default human-readable text line for this symbol, used by <see cref="BarcodeSvgRenderer"/>
    /// when <see cref="BarcodeRenderOptions.Text"/> is not set.
    /// </summary>
    public string HumanReadableText { get; }
}
