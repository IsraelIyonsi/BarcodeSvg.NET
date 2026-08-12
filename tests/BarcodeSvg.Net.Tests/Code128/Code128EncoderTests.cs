using BarcodeSvg;

namespace BarcodeSvg.Net.Tests.Code128;

public sealed class Code128EncoderTests
{
    // Expected width sequences below were computed by an independently written Python
    // reference encoder (not derived from this library) following the ISO/IEC 15417 symbol
    // table, checksum formula and the same optimal code-set switching rules, then cross-checked
    // by hand for the checksum arithmetic on the "PJJ123C" case. Polarity always starts on a bar
    // and alternates strictly, so only the width sequence is needed to pin the exact pattern.
    // Non-printable characters are written as explicit \uXXXX escapes so the source file stays
    // plain text (Git would otherwise flag a file containing a literal NUL byte as binary).
    public static TheoryData<string, string> ExactPatternFixtures => new()
    {
        // Mixed alphanumeric, digit run of 3 stays under the Code C threshold: pure Code Set B, no switches.
        { "PJJ123C", "2,1,1,2,1,4,3,1,3,1,2,1,1,1,2,1,3,3,1,1,2,1,3,3,1,2,3,2,2,1,2,2,3,2,1,1,2,2,1,1,3,2,1,3,1,3,2,1,3,1,1,3,2,1,2,3,3,1,1,1,2" },

        // Pure even-length digits: starts and stays in Code Set C for the whole payload.
        { "1234567890", "2,1,1,2,3,2,1,1,2,2,3,2,1,3,1,1,2,3,3,3,1,1,2,1,2,4,1,1,1,2,2,1,4,1,2,1,1,2,4,2,1,1,2,3,3,1,1,1,2" },

        // Letters, then an even (6-digit) run that clears the Code C threshold, then letters again: B -> C -> B.
        { "AB123456CD", "2,1,1,2,1,4,1,1,1,3,2,3,1,3,1,1,2,3,1,1,3,1,4,1,1,1,2,2,3,2,1,3,1,1,2,3,3,3,1,1,2,1,1,1,4,1,3,1,1,3,1,3,2,1,1,1,2,3,1,3,1,3,1,1,4,1,2,3,3,1,1,1,2" },

        // An odd (7-digit) run: 3 digit pairs packed in Code C, one leftover digit encoded back in Code Set B.
        { "AB1234567CD", "2,1,1,2,1,4,1,1,1,3,2,3,1,3,1,1,2,3,1,1,3,1,4,1,1,1,2,2,3,2,1,3,1,1,2,3,3,3,1,1,2,1,1,1,4,1,3,1,3,1,2,1,3,1,1,3,1,3,2,1,1,1,2,3,1,3,2,3,1,1,1,3,2,3,3,1,1,1,2" },

        // Leading control character (SOH, U+0001) forces the initial code set to A; the rest stays in A (no switch needed).
        { "\u0001ABC", "2,1,1,4,1,2,1,2,1,1,2,4,1,1,1,3,2,3,1,3,1,1,2,3,1,3,1,3,2,1,1,1,1,4,2,2,2,3,3,1,1,1,2" },

        // A single isolated Code Set A character (SOH, U+0001) amid Code Set B text: SHIFT for one character, then revert.
        { "ab\u0001cd", "2,1,1,2,1,4,1,2,1,1,2,4,1,2,1,4,2,1,4,1,1,3,1,1,1,2,1,1,2,4,1,4,1,1,2,2,1,4,1,2,2,1,2,1,3,1,3,1,2,3,3,1,1,1,2" },

        // Two consecutive Code Set A characters (SOH, STX, U+0001-U+0002) amid Code Set B text: LATCH to A, then LATCH back to B.
        { "ab\u0001\u0002cd", "2,1,1,2,1,4,1,2,1,1,2,4,1,2,1,4,2,1,3,1,1,1,4,1,1,2,1,1,2,4,1,2,1,4,2,1,1,1,4,1,3,1,1,4,1,1,2,2,1,4,1,2,2,1,3,2,1,2,2,1,2,3,3,1,1,1,2" },

        // ISO/IEC 15417 Annex E optimal start-set look-ahead: a shared character (space) is
        // immediately followed by characters that exclusively require Code Set A. Deciding the
        // start set from data[0] alone (space, shared) would default to B and need a LATCH to A
        // before the first control character, an 8-symbol-character encoding; looking ahead past
        // the shared space to the first set-exclusive character starts directly in A and needs no
        // latch at all, the ISO-optimal 7 symbol characters (Start, space, 3 controls, check, Stop).
        { " \u0001\u0002\u0003", "2,1,1,4,1,2,2,1,2,2,2,2,1,2,1,1,2,4,1,2,1,4,2,1,1,4,1,1,2,2,1,2,1,1,4,2,2,3,3,1,1,1,2" },

        // ISO/IEC 15417 Annex E optimal look-ahead after leaving Code Set C: once the "1234" digit
        // pair run ends, the next character ('A') is shared and satisfied by either set, but the
        // character after it (SOH) exclusively requires A. Deciding 'A's set without looking past
        // it would default to B, forcing a SHIFT to reach the trailing control character afterwards;
        // looking ahead finds the control character's A requirement and latches to A immediately,
        // saving that SHIFT (8 vs. 9 symbol characters for the unoptimized latch-then-shift).
        { "1234A\u0001", "2,1,1,2,3,2,1,1,2,2,3,2,1,3,1,1,2,3,3,1,1,1,4,1,1,1,1,3,2,3,1,2,1,1,2,4,2,2,3,2,1,1,2,3,3,1,1,1,2" },

        // Digit run one below the Code C threshold: stays entirely in Code Set B.
        { "123", "2,1,1,2,1,4,1,2,3,2,2,1,2,2,3,2,1,1,2,2,1,1,3,2,1,3,2,2,1,2,2,3,3,1,1,1,2" },

        // Digit run exactly at the Code C threshold: switches to Code Set C from the very start.
        { "1234", "2,1,1,2,3,2,1,1,2,2,3,2,1,3,1,1,2,3,1,2,1,2,4,1,2,3,3,1,1,1,2" },

        // ASCII range boundaries: NUL (U+0000, lowest Code Set A value) and DEL (U+007F, highest Code Set B value).
        { "\u0000", "2,1,1,4,1,2,1,1,1,4,2,2,1,1,1,4,2,2,2,3,3,1,1,1,2" },
        { "\u007F", "2,1,1,2,1,4,1,1,4,1,1,3,1,1,4,3,1,1,2,3,3,1,1,1,2" },

        // ASCII range boundaries: space (lowest shared value) and underscore (highest shared value).
        { " ", "2,1,1,2,1,4,2,1,2,2,2,2,2,2,2,1,2,2,2,3,3,1,1,1,2" },
        { "_", "2,1,1,2,1,4,1,1,1,2,2,4,1,1,1,4,2,2,2,3,3,1,1,1,2" },

        // ASCII range boundary: backtick is the lowest value requiring Code Set B above the shared range; tilde is the highest.
        { "`", "2,1,1,2,1,4,1,1,1,4,2,2,1,2,1,1,2,4,2,3,3,1,1,1,2" },
        { "~", "2,1,1,2,1,4,1,3,1,1,4,1,1,1,4,1,1,3,2,3,3,1,1,1,2" },
    };

    [Theory]
    [MemberData(nameof(ExactPatternFixtures))]
    public void Encode_ProducesExactBarPattern(string input, string expectedWidths)
    {
        var pattern = Code128Encoder.Encode(input);

        Assert.Equal(expectedWidths, WidthsToString(pattern));
    }

    [Theory]
    [MemberData(nameof(ExactPatternFixtures))]
    public void Encode_BarsAlwaysStartOnABarAndStrictlyAlternate(string input, string _)
    {
        var pattern = Code128Encoder.Encode(input);

        Assert.True(pattern.Bars[0].IsBar);
        for (var i = 1; i < pattern.Bars.Count; i++)
        {
            Assert.NotEqual(pattern.Bars[i - 1].IsBar, pattern.Bars[i].IsBar);
        }
    }

    [Theory]
    [MemberData(nameof(ExactPatternFixtures))]
    public void Encode_ReportsCode128RecommendedQuietZone(string input, string _)
    {
        var pattern = Code128Encoder.Encode(input);

        Assert.Equal(10, pattern.RecommendedQuietZoneModules);
    }

    [Theory]
    [MemberData(nameof(ExactPatternFixtures))]
    public void Encode_HumanReadableTextIsTheOriginalData(string input, string _)
    {
        var pattern = Code128Encoder.Encode(input);

        Assert.Equal(input, pattern.HumanReadableText);
    }

    [Fact]
    public void Encode_NullData_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => Code128Encoder.Encode(null!));
    }

    [Fact]
    public void Encode_EmptyData_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => Code128Encoder.Encode(string.Empty));
    }

    [Theory]
    [InlineData("café")]
    [InlineData("☃")]
    public void Encode_CharacterAboveAscii127_ThrowsArgumentException(string data)
    {
        Assert.Throws<ArgumentException>(() => Code128Encoder.Encode(data));
    }

    private static string WidthsToString(BarcodePattern pattern) =>
        string.Join(',', pattern.Bars.Select(b => b.WidthInModules));
}
