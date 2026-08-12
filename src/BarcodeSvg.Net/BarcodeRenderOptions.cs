namespace BarcodeSvg;

/// <summary>
/// Sizing, color and human-readable text options for <see cref="BarcodeSvgRenderer"/>.
/// </summary>
public sealed class BarcodeRenderOptions
{
    /// <summary>The module width used when neither <see cref="Width"/> nor <see cref="ModuleWidth"/> is set.</summary>
    public const double DefaultModuleWidth = 2.0;

    /// <summary>The bar height used when <see cref="Height"/> is not set.</summary>
    public const double DefaultHeight = 80.0;

    /// <summary>The text font size used when <see cref="FontSize"/> is not set.</summary>
    public const double DefaultFontSize = 12.0;

    /// <summary>The vertical gap between the bars and the human-readable text line.</summary>
    public const double DefaultTextMargin = 4.0;

    /// <summary>The bar and text color used when <see cref="BarColor"/> is not set.</summary>
    public const string DefaultBarColor = "#000000";

    /// <summary>The font family used when <see cref="FontFamily"/> is not set.</summary>
    public const string DefaultFontFamily = "monospace";

    /// <summary>
    /// The width, in SVG user units, of the bars area (excluding quiet zones). Mutually
    /// exclusive with <see cref="ModuleWidth"/>: set at most one of the two. When neither is
    /// set, <see cref="DefaultModuleWidth"/> is used.
    /// </summary>
    public double? Width { get; init; }

    /// <summary>
    /// The width, in SVG user units, of a single module (the narrowest bar or space). Mutually
    /// exclusive with <see cref="Width"/>: set at most one of the two. When neither is set,
    /// <see cref="DefaultModuleWidth"/> is used.
    /// </summary>
    public double? ModuleWidth { get; init; }

    /// <summary>The height, in SVG user units, of the bars. Defaults to <see cref="DefaultHeight"/>.</summary>
    public double Height { get; init; } = DefaultHeight;

    /// <summary>
    /// The quiet zone width, in SVG user units, applied on each side of the symbol. When not
    /// set, the symbology's recommended quiet zone (<see cref="BarcodePattern.RecommendedQuietZoneModules"/>)
    /// is used, scaled by the effective module width.
    /// </summary>
    public double? QuietZoneWidth { get; init; }

    /// <summary>Whether to render a human-readable text line beneath the bars. Defaults to <see langword="true"/>.</summary>
    public bool ShowText { get; init; } = true;

    /// <summary>
    /// The human-readable text to render beneath the bars. When not set, defaults to
    /// <see cref="BarcodePattern.HumanReadableText"/>. Ignored when <see cref="ShowText"/> is <see langword="false"/>.
    /// </summary>
    public string? Text { get; init; }

    /// <summary>The fill color for bars and text, as an SVG color value. Defaults to <see cref="DefaultBarColor"/>.</summary>
    public string BarColor { get; init; } = DefaultBarColor;

    /// <summary>
    /// The background color painted behind the whole symbol, as an SVG color value. When
    /// <see langword="null"/> (the default), no background rectangle is drawn and the SVG is transparent.
    /// </summary>
    public string? BackgroundColor { get; init; }

    /// <summary>The font size, in SVG user units, of the human-readable text. Defaults to <see cref="DefaultFontSize"/>.</summary>
    public double FontSize { get; init; } = DefaultFontSize;

    /// <summary>The font family of the human-readable text. Defaults to <see cref="DefaultFontFamily"/>.</summary>
    public string FontFamily { get; init; } = DefaultFontFamily;
}
