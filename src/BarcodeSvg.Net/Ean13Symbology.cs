namespace BarcodeSvg;

/// <summary>
/// The digit tables, parity table and layout constants for the EAN-13 symbology (ISO/IEC 15420),
/// shared by <see cref="Ean13Encoder"/>.
/// </summary>
internal static class Ean13Symbology
{
    /// <summary>The number of digits a caller supplies when the check digit will be computed.</summary>
    internal const int DataDigitCount = 12;

    /// <summary>The number of digits in a complete EAN-13 value, including the check digit.</summary>
    internal const int FullDigitCount = 13;

    /// <summary>The number of digits encoded in each of the left-hand and right-hand groups.</summary>
    internal const int DigitsPerGroup = 6;

    /// <summary>The number of modules each digit's encoding occupies.</summary>
    internal const int DigitWidthInModules = 7;

    /// <summary>
    /// The total module width of a complete EAN-13 symbol: two 3-module guard patterns, one
    /// 5-module center guard pattern, and twelve 7-module digit patterns (2 * 3 + 5 + 12 * 7).
    /// </summary>
    internal const int TotalModules = (2 * 3) + 5 + (2 * DigitsPerGroup * DigitWidthInModules);

    /// <summary>The check digit weight applied to odd 1-based digit positions.</summary>
    internal const int OddPositionWeight = 1;

    /// <summary>The check digit weight applied to even 1-based digit positions.</summary>
    internal const int EvenPositionWeight = 3;

    /// <summary>The modulus used to compute the EAN-13 check digit.</summary>
    internal const int ChecksumModulus = 10;

    /// <summary>
    /// The quiet zone width recommended for EAN-13, in modules, applied on each side of the
    /// symbol when a renderer is not given an explicit override.
    /// </summary>
    internal const int RecommendedQuietZoneModules = 9;

    /// <summary>The start/end guard pattern: bar, space, bar, all one module wide.</summary>
    internal static readonly bool[] GuardPattern = { true, false, true };

    /// <summary>The center guard pattern: space, bar, space, bar, space, all one module wide.</summary>
    internal static readonly bool[] CenterGuardPattern = { false, true, false, true, false };

    /// <summary>
    /// Left-hand odd-parity (L-code) digit patterns for digits 0-9, each 7 modules read as a
    /// bar (<see langword="true"/>) or space (<see langword="false"/>) per module.
    /// </summary>
    internal static readonly bool[][] LeftOddPatterns =
    {
        ParseModules("0001101"), // 0
        ParseModules("0011001"), // 1
        ParseModules("0010011"), // 2
        ParseModules("0111101"), // 3
        ParseModules("0100011"), // 4
        ParseModules("0110001"), // 5
        ParseModules("0101111"), // 6
        ParseModules("0111011"), // 7
        ParseModules("0110111"), // 8
        ParseModules("0001011"), // 9
    };

    /// <summary>
    /// Left-hand even-parity (G-code) digit patterns for digits 0-9, each 7 modules read as a
    /// bar (<see langword="true"/>) or space (<see langword="false"/>) per module.
    /// </summary>
    internal static readonly bool[][] LeftEvenPatterns =
    {
        ParseModules("0100111"), // 0
        ParseModules("0110011"), // 1
        ParseModules("0011011"), // 2
        ParseModules("0100001"), // 3
        ParseModules("0011101"), // 4
        ParseModules("0111001"), // 5
        ParseModules("0000101"), // 6
        ParseModules("0010001"), // 7
        ParseModules("0001001"), // 8
        ParseModules("0010111"), // 9
    };

    /// <summary>
    /// Right-hand (R-code) digit patterns for digits 0-9, each 7 modules read as a bar
    /// (<see langword="true"/>) or space (<see langword="false"/>) per module.
    /// </summary>
    internal static readonly bool[][] RightPatterns =
    {
        ParseModules("1110010"), // 0
        ParseModules("1100110"), // 1
        ParseModules("1101100"), // 2
        ParseModules("1000010"), // 3
        ParseModules("1011100"), // 4
        ParseModules("1001110"), // 5
        ParseModules("1010000"), // 6
        ParseModules("1000100"), // 7
        ParseModules("1001000"), // 8
        ParseModules("1110100"), // 9
    };

    /// <summary>
    /// For each possible leading (number system) digit 0-9, the sequence of parity codes
    /// (<see langword="false"/> = L, <see langword="true"/> = G) applied to the six left-hand
    /// digits that follow it.
    /// </summary>
    internal static readonly bool[][] LeftGroupParityByFirstDigit =
    {
        ParseParity("LLLLLL"), // 0
        ParseParity("LLGLGG"), // 1
        ParseParity("LLGGLG"), // 2
        ParseParity("LLGGGL"), // 3
        ParseParity("LGLLGG"), // 4
        ParseParity("LGGLLG"), // 5
        ParseParity("LGGGLL"), // 6
        ParseParity("LGLGLG"), // 7
        ParseParity("LGLGGL"), // 8
        ParseParity("LGGLGL"), // 9
    };

    private static bool[] ParseModules(string bits)
    {
        var modules = new bool[bits.Length];
        for (var i = 0; i < bits.Length; i++)
        {
            modules[i] = bits[i] == '1';
        }

        return modules;
    }

    private static bool[] ParseParity(string code)
    {
        var parity = new bool[code.Length];
        for (var i = 0; i < code.Length; i++)
        {
            parity[i] = code[i] == 'G';
        }

        return parity;
    }
}
