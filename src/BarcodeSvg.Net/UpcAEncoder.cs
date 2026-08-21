namespace BarcodeSvg;

/// <summary>
/// Encodes an 11 or 12 digit value into a UPC-A (ISO/IEC 15420) bar/space pattern, computing or
/// validating the mandatory check digit.
/// </summary>
/// <remarks>
/// UPC-A is the EAN-13 symbol with an implicit leading number-system digit of <c>0</c>: the encoded
/// bars and the weighted modulo-10 check digit of a 12-digit UPC-A value are identical to those of
/// the EAN-13 value <see cref="UpcASymbology.Ean13NumberSystemPrefix"/> + the 12 digits. This encoder
/// therefore reuses <see cref="Ean13Encoder"/> for the digit tables, guard bars, parity table and
/// checksum, and differs only in accepting 11/12 digits and in reporting the human-readable line as
/// the 12-digit UPC-A value rather than the 13-digit EAN-13 value.
/// </remarks>
public static class UpcAEncoder
{
    /// <summary>
    /// Computes the UPC-A check digit for an 11-digit value, using the standard weighted modulo-10
    /// algorithm (identical in result to the EAN-13 check digit of the same digits prefixed with the
    /// number-system <c>0</c>).
    /// </summary>
    /// <param name="digits">Exactly 11 ASCII digit characters.</param>
    /// <returns>The check digit, 0-9.</returns>
    /// <exception cref="ArgumentException">
    /// <paramref name="digits"/> is not exactly 11 characters long, or contains a non-digit character.
    /// </exception>
    public static int ComputeCheckDigit(ReadOnlySpan<char> digits)
    {
        if (digits.Length != UpcASymbology.DataDigitCount)
        {
            throw new ArgumentException(
                $"Expected exactly {UpcASymbology.DataDigitCount} digits, got {digits.Length}.",
                nameof(digits));
        }

        return Ean13Encoder.ComputeCheckDigit(UpcASymbology.Ean13NumberSystemPrefix + digits.ToString());
    }

    /// <summary>
    /// Encodes <paramref name="digits"/> as a UPC-A symbol.
    /// </summary>
    /// <param name="digits">
    /// Either 11 ASCII digits (the check digit is computed and appended) or 12 ASCII digits (the
    /// 12th digit is validated against the computed check digit).
    /// </param>
    /// <returns>
    /// The resulting bar/space pattern. The bars are exactly those of the equivalent EAN-13 symbol
    /// (<see cref="UpcASymbology.Ean13NumberSystemPrefix"/> + the 12-digit value), while the
    /// human-readable text is the 12-digit UPC-A value.
    /// </returns>
    /// <exception cref="ArgumentNullException"><paramref name="digits"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="digits"/> is not 11 or 12 characters long, or contains a non-digit character.
    /// </exception>
    /// <exception cref="FormatException">
    /// <paramref name="digits"/> is 12 characters long and its last digit does not match the computed
    /// check digit.
    /// </exception>
    public static BarcodePattern Encode(string digits)
    {
        ArgumentNullException.ThrowIfNull(digits);

        string fullDigits;
        if (digits.Length == UpcASymbology.DataDigitCount)
        {
            ValidateAllDigits(digits);
            var checkDigit = ComputeCheckDigit(digits.AsSpan());
            fullDigits = digits + (char)('0' + checkDigit);
        }
        else if (digits.Length == UpcASymbology.FullDigitCount)
        {
            ValidateAllDigits(digits);
            var expectedCheckDigit = ComputeCheckDigit(digits.AsSpan(0, UpcASymbology.DataDigitCount));
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
                $"UPC-A input must be exactly {UpcASymbology.DataDigitCount} digits (check digit computed) " +
                $"or {UpcASymbology.FullDigitCount} digits (check digit validated), not {digits.Length}.",
                nameof(digits));
        }

        var ean13 = Ean13Encoder.Encode(UpcASymbology.Ean13NumberSystemPrefix + fullDigits);
        return new BarcodePattern(ean13.Bars, ean13.RecommendedQuietZoneModules, fullDigits);
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
}
