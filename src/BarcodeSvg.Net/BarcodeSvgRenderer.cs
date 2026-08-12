using System.Globalization;
using System.Text;

namespace BarcodeSvg;

/// <summary>
/// Renders a <see cref="BarcodePattern"/> as a self-contained SVG string: no external
/// stylesheets, fonts, images or scripts, so the output can be embedded directly into a page,
/// PDF or email without further processing.
/// </summary>
public static class BarcodeSvgRenderer
{
    private const string NumberFormat = "0.###";
    private const string SvgNamespace = "http://www.w3.org/2000/svg";

    /// <summary>
    /// Renders <paramref name="pattern"/> as an SVG document string.
    /// </summary>
    /// <param name="pattern">The bar/space pattern to render, from <see cref="Code128Encoder"/> or <see cref="Ean13Encoder"/>.</param>
    /// <param name="options">Sizing, color and text options. When <see langword="null"/>, default options are used.</param>
    /// <returns>
    /// A well-formed, self-contained SVG document string. Rendering the same pattern with the
    /// same options always produces the exact same string.
    /// </returns>
    /// <exception cref="ArgumentNullException"><paramref name="pattern"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException"><paramref name="options"/> contains an invalid combination or value.</exception>
    public static string Render(BarcodePattern pattern, BarcodeRenderOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(pattern);
        options ??= new BarcodeRenderOptions();
        ValidateOptions(options);

        var moduleWidth = ResolveModuleWidth(pattern, options);
        var quietZoneWidth = options.QuietZoneWidth ?? (pattern.RecommendedQuietZoneModules * moduleWidth);
        var barsAreaWidth = pattern.TotalModules * moduleWidth;
        var totalWidth = barsAreaWidth + (2 * quietZoneWidth);

        var text = options.ShowText ? options.Text ?? pattern.HumanReadableText : null;
        var hasText = !string.IsNullOrEmpty(text);
        var totalHeight = options.Height + (hasText ? options.FontSize + BarcodeRenderOptions.DefaultTextMargin : 0);

        var svg = new StringBuilder();
        svg.Append("<svg xmlns=\"").Append(SvgNamespace).Append('"')
           .Append(" width=\"").Append(FormatNumber(totalWidth)).Append('"')
           .Append(" height=\"").Append(FormatNumber(totalHeight)).Append('"')
           .Append(" viewBox=\"0 0 ").Append(FormatNumber(totalWidth)).Append(' ').Append(FormatNumber(totalHeight)).Append('"')
           .Append('>');

        if (!string.IsNullOrEmpty(options.BackgroundColor))
        {
            AppendRect(svg, 0, 0, totalWidth, totalHeight, options.BackgroundColor);
        }

        var barColor = options.BarColor;
        var x = quietZoneWidth;
        foreach (var bar in pattern.Bars)
        {
            var width = bar.WidthInModules * moduleWidth;
            if (bar.IsBar)
            {
                AppendRect(svg, x, 0, width, options.Height, barColor);
            }

            x += width;
        }

        if (hasText)
        {
            svg.Append("<text x=\"").Append(FormatNumber(totalWidth / 2)).Append('"')
               .Append(" y=\"").Append(FormatNumber(options.Height + options.FontSize)).Append('"')
               .Append(" text-anchor=\"middle\"")
               .Append(" font-family=\"").Append(XmlEscaper.Escape(options.FontFamily)).Append('"')
               .Append(" font-size=\"").Append(FormatNumber(options.FontSize)).Append('"')
               .Append(" fill=\"").Append(XmlEscaper.Escape(barColor)).Append('"')
               .Append('>')
               .Append(XmlEscaper.Escape(text!))
               .Append("</text>");
        }

        svg.Append("</svg>");
        return svg.ToString();
    }

    private static void AppendRect(StringBuilder svg, double x, double y, double width, double height, string fill)
    {
        svg.Append("<rect x=\"").Append(FormatNumber(x)).Append('"')
           .Append(" y=\"").Append(FormatNumber(y)).Append('"')
           .Append(" width=\"").Append(FormatNumber(width)).Append('"')
           .Append(" height=\"").Append(FormatNumber(height)).Append('"')
           .Append(" fill=\"").Append(XmlEscaper.Escape(fill)).Append('"')
           .Append("/>");
    }

    private static double ResolveModuleWidth(BarcodePattern pattern, BarcodeRenderOptions options)
    {
        if (options.ModuleWidth.HasValue)
        {
            return options.ModuleWidth.Value;
        }

        if (options.Width.HasValue)
        {
            return options.Width.Value / pattern.TotalModules;
        }

        return BarcodeRenderOptions.DefaultModuleWidth;
    }

    private static void ValidateOptions(BarcodeRenderOptions options)
    {
        if (options.Width.HasValue && options.ModuleWidth.HasValue)
        {
            throw new ArgumentException("Set at most one of Width and ModuleWidth, not both.", nameof(options));
        }

        if (options.Width is <= 0)
        {
            throw new ArgumentException("Width must be positive.", nameof(options));
        }

        if (options.ModuleWidth is <= 0)
        {
            throw new ArgumentException("ModuleWidth must be positive.", nameof(options));
        }

        if (options.Height <= 0)
        {
            throw new ArgumentException("Height must be positive.", nameof(options));
        }

        if (options.QuietZoneWidth is < 0)
        {
            throw new ArgumentException("QuietZoneWidth cannot be negative.", nameof(options));
        }

        if (options.FontSize <= 0)
        {
            throw new ArgumentException("FontSize must be positive.", nameof(options));
        }

        if (string.IsNullOrEmpty(options.BarColor))
        {
            throw new ArgumentException("BarColor must not be empty.", nameof(options));
        }

        if (string.IsNullOrEmpty(options.FontFamily))
        {
            throw new ArgumentException("FontFamily must not be empty.", nameof(options));
        }
    }

    private static string FormatNumber(double value) => value.ToString(NumberFormat, CultureInfo.InvariantCulture);
}
