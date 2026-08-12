using System.Text;

namespace BarcodeSvg;

/// <summary>
/// Escapes text so it is safe to embed as either XML element content or a quoted XML attribute
/// value, preventing markup injection from untrusted barcode content or human-readable text.
/// </summary>
/// <remarks>
/// Code 128 explicitly supports encoding ASCII control characters (Code Set A), so the human
/// readable text this class escapes can legitimately contain bytes that are not well-formed XML
/// 1.0 <c>Char</c> data: 0x00-0x08, 0x0B, 0x0C and 0x0E-0x1F are all illegal in XML 1.0 content,
/// and (unlike <c>&amp;</c> or <c>&lt;</c>) there is no legal numeric character reference for them
/// either, so they cannot be escaped, only removed or replaced with a character that IS legal.
/// This class substitutes them with U+FFFD (the standard Unicode replacement character) so the
/// output stays well-formed XML while still visibly signalling that data was present there.
/// </remarks>
internal static class XmlEscaper
{
    /// <summary>
    /// The character substituted for any input character that has no well-formed XML 1.0
    /// representation, escaped or otherwise.
    /// </summary>
    private const char ReplacementCharacter = '\uFFFD';

    /// <summary>Inclusive upper bound of the C0 control character range XML 1.0 restricts.</summary>
    private const char C0ControlUpperBoundInclusive = '\u001F';

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
                    builder.Append(IsXmlIllegalControlCharacter(c) ? ReplacementCharacter : c);
                    break;
            }
        }

        return builder.ToString();
    }

    /// <summary>
    /// Reports whether <paramref name="c"/> falls in one of the C0 control character gaps the
    /// XML 1.0 <c>Char</c> production excludes: 0x00-0x08, 0x0B, 0x0C and 0x0E-0x1F. The three
    /// C0 controls XML does allow, tab (0x09), line feed (0x0A) and carriage return (0x0D), are
    /// deliberately excluded here.
    /// </summary>
    private static bool IsXmlIllegalControlCharacter(char c) =>
        c <= C0ControlUpperBoundInclusive && c != '\t' && c != '\n' && c != '\r';
}
