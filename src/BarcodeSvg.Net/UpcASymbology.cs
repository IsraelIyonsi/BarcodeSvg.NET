namespace BarcodeSvg;

/// <summary>
/// The layout constants for the UPC-A symbology (ISO/IEC 15420). UPC-A is structurally the EAN-13
/// symbol whose leading number-system digit is <c>0</c>: a 12-digit UPC-A value encodes and checks
/// exactly like the EAN-13 of <see cref="Ean13NumberSystemPrefix"/> followed by those 12 digits, so
/// <see cref="UpcAEncoder"/> delegates encoding and check-digit maths to <see cref="Ean13Encoder"/>
/// and this type holds only what differs: the shorter digit counts and that prefix.
/// </summary>
internal static class UpcASymbology
{
    /// <summary>The number of digits a caller supplies when the check digit will be computed.</summary>
    internal const int DataDigitCount = 11;

    /// <summary>The number of digits in a complete UPC-A value, including the check digit.</summary>
    internal const int FullDigitCount = 12;

    /// <summary>
    /// The EAN-13 number-system digit that a UPC-A symbol implicitly carries. Prefixing it to a
    /// 12-digit UPC-A value yields the equivalent 13-digit EAN-13 value that produces the identical
    /// bar/space pattern and the identical check digit.
    /// </summary>
    internal const string Ean13NumberSystemPrefix = "0";
}
