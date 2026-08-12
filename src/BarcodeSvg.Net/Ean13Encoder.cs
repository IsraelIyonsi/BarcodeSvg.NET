namespace BarcodeSvg;

/// <summary>
/// Encodes a 12 or 13 digit value into an EAN-13 (ISO/IEC 15420) bar/space pattern, computing or
/// validating the mandatory check digit and applying the left-hand parity pattern driven by the
/// leading digit.
/// </summary>
public static class Ean13Encoder
{
    private const int SingleModuleWidth = 1;

    /// <summary>
    /// Computes the EAN-13 check digit for a 12-digit value, using the standard weighted
    /// modulo-10 algorithm: digits at odd 1-based positions are weighted 1, digits at even
    /// positions are weighted 3, and the check digit is whatever makes the total sum a multiple
    /// of 10.
    /// </summary>
    /// <param name="digits">Exactly 12 ASCII digit characters.</param>
    /// <returns>The check digit, 0-9.</returns>
    /// <exception cref="ArgumentException">
    /// <paramref name="digits"/> is not exactly 12 characters long, or contains a non-digit character.
    /// </exception>
    public static int ComputeCheckDigit(ReadOnlySpan<char> digits)
    {
        if (digits.Length != Ean13Symbology.DataDigitCount)
        {
            throw new ArgumentException(
                $"Expected exactly {Ean13Symbology.DataDigitCount} digits, got {digits.Length}.",
                nameof(digits));
        }

        var sum = 0;
        for (var i = 0; i < digits.Length; i++)
        {
            if (!char.IsAsciiDigit(digits[i]))
            {
                throw new ArgumentException($"'{digits[i]}' is not an ASCII digit.", nameof(digits));
            }

            var position = i + 1;
            var weight = position % 2 == 0 ? Ean13Symbology.EvenPositionWeight : Ean13Symbology.OddPositionWeight;
            sum += (digits[i] - '0') * weight;
        }

        return (Ean13Symbology.ChecksumModulus - (sum % Ean13Symbology.ChecksumModulus)) % Ean13Symbology.ChecksumModulus;
    }

    /// <summary>
    /// Encodes <paramref name="digits"/> as an EAN-13 symbol.
    /// </summary>
    /// <param name="digits">
    /// Either 12 ASCII digits (the check digit is computed and appended) or 13 ASCII digits (the
    /// 13th digit is validated against the computed check digit).
    /// </param>
    /// <returns>The resulting bar/space pattern.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="digits"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="digits"/> is not 12 or 13 characters long, or contains a non-digit character.
    /// </exception>
    /// <exception cref="FormatException">
    /// <paramref name="digits"/> is 13 characters long and its last digit does not match the
    /// computed check digit.
    /// </exception>
    public static BarcodePattern Encode(string digits)
    {
        ArgumentNullException.ThrowIfNull(digits);

        string fullDigits;
        if (digits.Length == Ean13Symbology.DataDigitCount)
        {
            ValidateAllDigits(digits);
            var checkDigit = ComputeCheckDigit(digits.AsSpan());
            fullDigits = digits + (char)('0' + checkDigit);
        }
        else if (digits.Length == Ean13Symbology.FullDigitCount)
        {
            ValidateAllDigits(digits);
            var expectedCheckDigit = ComputeCheckDigit(digits.AsSpan(0, Ean13Symbology.DataDigitCount));
            var suppliedCheckDigit = digits[^1] - '0';
            if (suppliedCheckDigit != expectedCheckDigit)
            {
                throw new FormatException(
                    $"Check digit mismatch: '{digits}' ends in {suppliedCheckDigit} but {expectedCheckDigit} was expected.");
            }

            fullDigits = digits;
        }
        else
        {
            throw new ArgumentException(
                $"EAN-13 input must be exactly {Ean13Symbology.DataDigitCount} digits (check digit computed) " +
                $"or {Ean13Symbology.FullDigitCount} digits (check digit validated), not {digits.Length}.",
                nameof(digits));
        }

        var bars = BuildBarSegments(fullDigits);
        return new BarcodePattern(bars, Ean13Symbology.RecommendedQuietZoneModules, fullDigits);
    }

    private static void ValidateAllDigits(string digits)
    {
        foreach (var c in digits)
        {
            if (!char.IsAsciiDigit(c))
            {
                throw new ArgumentException($"'{c}' is not an ASCII digit.", nameof(digits));
            }
        }
    }

    private static List<BarSegment> BuildBarSegments(string fullDigits)
    {
        var bars = new List<BarSegment>();
        AppendModules(bars, Ean13Symbology.GuardPattern);

        var firstDigit = fullDigits[0] - '0';
        var parity = Ean13Symbology.LeftGroupParityByFirstDigit[firstDigit];
        for (var i = 0; i < Ean13Symbology.DigitsPerGroup; i++)
        {
            var digit = fullDigits[1 + i] - '0';
            var pattern = parity[i] ? Ean13Symbology.LeftEvenPatterns[digit] : Ean13Symbology.LeftOddPatterns[digit];
            AppendModules(bars, pattern);
        }

        AppendModules(bars, Ean13Symbology.CenterGuardPattern);

        for (var i = 0; i < Ean13Symbology.DigitsPerGroup; i++)
        {
            var digit = fullDigits[1 + Ean13Symbology.DigitsPerGroup + i] - '0';
            AppendModules(bars, Ean13Symbology.RightPatterns[digit]);
        }

        AppendModules(bars, Ean13Symbology.GuardPattern);

        return bars;
    }

    private static void AppendModules(List<BarSegment> bars, bool[] modules)
    {
        foreach (var isBar in modules)
        {
            bars.Add(new BarSegment(isBar, SingleModuleWidth));
        }
    }
}
