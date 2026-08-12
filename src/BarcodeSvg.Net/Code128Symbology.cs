namespace BarcodeSvg;

/// <summary>
/// The symbol table, symbol-value constants and layout constants for the Code 128 symbology
/// (ISO/IEC 15417), shared by <see cref="Code128Encoder"/>.
/// </summary>
internal static class Code128Symbology
{
    /// <summary>The number of modules every symbol character (other than STOP) occupies.</summary>
    internal const int SymbolWidthInModules = 11;

    /// <summary>The number of modules the STOP character occupies.</summary>
    internal const int StopWidthInModules = 13;

    /// <summary>The lowest ASCII code point encodable in Code Set A (control characters).</summary>
    internal const int ControlCharacterUpperBoundExclusive = 32;

    /// <summary>The highest ASCII code point shared by Code Set A and Code Set B.</summary>
    internal const int SharedCharacterUpperBoundInclusive = 95;

    /// <summary>The highest ASCII code point encodable in Code 128 (Code Set B only, above the shared range).</summary>
    internal const int MaxSupportedAsciiValue = 127;

    /// <summary>The offset subtracted from a Code Set B ASCII value to get its symbol value.</summary>
    internal const int CodeSetBAsciiOffset = 32;

    /// <summary>The offset added to a Code Set A control character to get its symbol value.</summary>
    internal const int CodeSetAControlOffset = 64;

    /// <summary>The minimum length of a digit run that makes switching to Code Set C worthwhile.</summary>
    internal const int MinimumCodeCDigitRun = 4;

    /// <summary>The number of digits Code Set C packs into a single symbol character.</summary>
    internal const int CodeCDigitsPerSymbol = 2;

    /// <summary>Symbol value that shifts the next single character to the other of A/B, then reverts.</summary>
    internal const int ShiftValue = 98;

    /// <summary>Symbol value that latches to Code Set C.</summary>
    internal const int CodeSetCValue = 99;

    /// <summary>Symbol value that latches to Code Set B.</summary>
    internal const int CodeSetBValue = 100;

    /// <summary>Symbol value that latches to Code Set A.</summary>
    internal const int CodeSetAValue = 101;

    /// <summary>Symbol value of the Start Code A character.</summary>
    internal const int StartAValue = 103;

    /// <summary>Symbol value of the Start Code B character.</summary>
    internal const int StartBValue = 104;

    /// <summary>Symbol value of the Start Code C character.</summary>
    internal const int StartCValue = 105;

    /// <summary>Symbol value of the Stop character.</summary>
    internal const int StopValue = 106;

    /// <summary>The modulus used to compute the Code 128 check character.</summary>
    internal const int ChecksumModulus = 103;

    /// <summary>
    /// Bar/space width patterns for symbol values 0 through 105, each a 6-element run of module
    /// widths alternating bar, space, bar, space, bar, space. Verified against the ISO/IEC 15417
    /// symbol character table.
    /// </summary>
    internal static readonly IReadOnlyList<int[]> Patterns = new int[][]
    {
        new[] { 2, 1, 2, 2, 2, 2 }, // 0
        new[] { 2, 2, 2, 1, 2, 2 }, // 1
        new[] { 2, 2, 2, 2, 2, 1 }, // 2
        new[] { 1, 2, 1, 2, 2, 3 }, // 3
        new[] { 1, 2, 1, 3, 2, 2 }, // 4
        new[] { 1, 3, 1, 2, 2, 2 }, // 5
        new[] { 1, 2, 2, 2, 1, 3 }, // 6
        new[] { 1, 2, 2, 3, 1, 2 }, // 7
        new[] { 1, 3, 2, 2, 1, 2 }, // 8
        new[] { 2, 2, 1, 2, 1, 3 }, // 9
        new[] { 2, 2, 1, 3, 1, 2 }, // 10
        new[] { 2, 3, 1, 2, 1, 2 }, // 11
        new[] { 1, 1, 2, 2, 3, 2 }, // 12
        new[] { 1, 2, 2, 1, 3, 2 }, // 13
        new[] { 1, 2, 2, 2, 3, 1 }, // 14
        new[] { 1, 1, 3, 2, 2, 2 }, // 15
        new[] { 1, 2, 3, 1, 2, 2 }, // 16
        new[] { 1, 2, 3, 2, 2, 1 }, // 17
        new[] { 2, 2, 3, 2, 1, 1 }, // 18
        new[] { 2, 2, 1, 1, 3, 2 }, // 19
        new[] { 2, 2, 1, 2, 3, 1 }, // 20
        new[] { 2, 1, 3, 2, 1, 2 }, // 21
        new[] { 2, 2, 3, 1, 1, 2 }, // 22
        new[] { 3, 1, 2, 1, 3, 1 }, // 23
        new[] { 3, 1, 1, 2, 2, 2 }, // 24
        new[] { 3, 2, 1, 1, 2, 2 }, // 25
        new[] { 3, 2, 1, 2, 2, 1 }, // 26
        new[] { 3, 1, 2, 2, 1, 2 }, // 27
        new[] { 3, 2, 2, 1, 1, 2 }, // 28
        new[] { 3, 2, 2, 2, 1, 1 }, // 29
        new[] { 2, 1, 2, 1, 2, 3 }, // 30
        new[] { 2, 1, 2, 3, 2, 1 }, // 31
        new[] { 2, 3, 2, 1, 2, 1 }, // 32
        new[] { 1, 1, 1, 3, 2, 3 }, // 33
        new[] { 1, 3, 1, 1, 2, 3 }, // 34
        new[] { 1, 3, 1, 3, 2, 1 }, // 35
        new[] { 1, 1, 2, 3, 1, 3 }, // 36
        new[] { 1, 3, 2, 1, 1, 3 }, // 37
        new[] { 1, 3, 2, 3, 1, 1 }, // 38
        new[] { 2, 1, 1, 3, 1, 3 }, // 39
        new[] { 2, 3, 1, 1, 1, 3 }, // 40
        new[] { 2, 3, 1, 3, 1, 1 }, // 41
        new[] { 1, 1, 2, 1, 3, 3 }, // 42
        new[] { 1, 1, 2, 3, 3, 1 }, // 43
        new[] { 1, 3, 2, 1, 3, 1 }, // 44
        new[] { 1, 1, 3, 1, 2, 3 }, // 45
        new[] { 1, 1, 3, 3, 2, 1 }, // 46
        new[] { 1, 3, 3, 1, 2, 1 }, // 47
        new[] { 3, 1, 3, 1, 2, 1 }, // 48
        new[] { 2, 1, 1, 3, 3, 1 }, // 49
        new[] { 2, 3, 1, 1, 3, 1 }, // 50
        new[] { 2, 1, 3, 1, 1, 3 }, // 51
        new[] { 2, 1, 3, 3, 1, 1 }, // 52
        new[] { 2, 1, 3, 1, 3, 1 }, // 53
        new[] { 3, 1, 1, 1, 2, 3 }, // 54
        new[] { 3, 1, 1, 3, 2, 1 }, // 55
        new[] { 3, 3, 1, 1, 2, 1 }, // 56
        new[] { 3, 1, 2, 1, 1, 3 }, // 57
        new[] { 3, 1, 2, 3, 1, 1 }, // 58
        new[] { 3, 3, 2, 1, 1, 1 }, // 59
        new[] { 3, 1, 4, 1, 1, 1 }, // 60
        new[] { 2, 2, 1, 4, 1, 1 }, // 61
        new[] { 4, 3, 1, 1, 1, 1 }, // 62
        new[] { 1, 1, 1, 2, 2, 4 }, // 63
        new[] { 1, 1, 1, 4, 2, 2 }, // 64
        new[] { 1, 2, 1, 1, 2, 4 }, // 65
        new[] { 1, 2, 1, 4, 2, 1 }, // 66
        new[] { 1, 4, 1, 1, 2, 2 }, // 67
        new[] { 1, 4, 1, 2, 2, 1 }, // 68
        new[] { 1, 1, 2, 2, 1, 4 }, // 69
        new[] { 1, 1, 2, 4, 1, 2 }, // 70
        new[] { 1, 2, 2, 1, 1, 4 }, // 71
        new[] { 1, 2, 2, 4, 1, 1 }, // 72
        new[] { 1, 4, 2, 1, 1, 2 }, // 73
        new[] { 1, 4, 2, 2, 1, 1 }, // 74
        new[] { 2, 4, 1, 2, 1, 1 }, // 75
        new[] { 2, 2, 1, 1, 1, 4 }, // 76
        new[] { 4, 1, 3, 1, 1, 1 }, // 77
        new[] { 2, 4, 1, 1, 1, 2 }, // 78
        new[] { 1, 3, 4, 1, 1, 1 }, // 79
        new[] { 1, 1, 1, 2, 4, 2 }, // 80
        new[] { 1, 2, 1, 1, 4, 2 }, // 81
        new[] { 1, 2, 1, 2, 4, 1 }, // 82
        new[] { 1, 1, 4, 2, 1, 2 }, // 83
        new[] { 1, 2, 4, 1, 1, 2 }, // 84
        new[] { 1, 2, 4, 2, 1, 1 }, // 85
        new[] { 4, 1, 1, 2, 1, 2 }, // 86
        new[] { 4, 2, 1, 1, 1, 2 }, // 87
        new[] { 4, 2, 1, 2, 1, 1 }, // 88
        new[] { 2, 1, 2, 1, 4, 1 }, // 89
        new[] { 2, 1, 4, 1, 2, 1 }, // 90
        new[] { 4, 1, 2, 1, 2, 1 }, // 91
        new[] { 1, 1, 1, 1, 4, 3 }, // 92
        new[] { 1, 1, 1, 3, 4, 1 }, // 93
        new[] { 1, 3, 1, 1, 4, 1 }, // 94
        new[] { 1, 1, 4, 1, 1, 3 }, // 95
        new[] { 1, 1, 4, 3, 1, 1 }, // 96
        new[] { 4, 1, 1, 1, 1, 3 }, // 97
        new[] { 4, 1, 1, 3, 1, 1 }, // 98
        new[] { 1, 1, 3, 1, 4, 1 }, // 99
        new[] { 1, 1, 4, 1, 3, 1 }, // 100
        new[] { 3, 1, 1, 1, 4, 1 }, // 101
        new[] { 4, 1, 1, 1, 3, 1 }, // 102
        new[] { 2, 1, 1, 4, 1, 2 }, // 103 Start A
        new[] { 2, 1, 1, 2, 1, 4 }, // 104 Start B
        new[] { 2, 1, 1, 2, 3, 2 }, // 105 Start C
    };

    /// <summary>
    /// The bar/space width pattern for the Stop character: 7 elements, alternating bar, space,
    /// bar, space, bar, space, bar. Wider than an ordinary symbol character by one trailing bar.
    /// </summary>
    internal static readonly int[] StopPattern = { 2, 3, 3, 1, 1, 1, 2 };
}
