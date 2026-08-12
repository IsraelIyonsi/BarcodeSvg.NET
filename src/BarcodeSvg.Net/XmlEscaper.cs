using System.Text;

namespace BarcodeSvg;

/// <summary>
/// Escapes text so it is safe to embed as either XML element content or a quoted XML attribute
/// value, preventing markup injection from untrusted barcode content or human-readable text.
/// </summary>
internal static class XmlEscaper
{
    internal static string Escape(string value)
    {
        var builder = new StringBuilder(value.Length);
        foreach (var c in value)
        {
            switch (c)
            {
                case '&':
                    builder.Append("&amp;");
                    break;
                case '<':
                    builder.Append("&lt;");
                    break;
                case '>':
                    builder.Append("&gt;");
                    break;
                case '"':
                    builder.Append("&quot;");
                    break;
                case '\'':
                    builder.Append("&apos;");
                    break;
                default:
                    builder.Append(c);
                    break;
            }
        }

        return builder.ToString();
    }
}
