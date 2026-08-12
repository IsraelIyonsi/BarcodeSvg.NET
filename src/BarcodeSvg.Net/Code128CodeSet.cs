namespace BarcodeSvg;

/// <summary>The three Code 128 code sets an encoder can be latched into at any point.</summary>
internal enum Code128CodeSet
{
    /// <summary>Uppercase letters, digits, punctuation and ASCII control characters.</summary>
    A,

    /// <summary>Uppercase and lowercase letters, digits and punctuation.</summary>
    B,

    /// <summary>Pairs of digits, two per symbol character.</summary>
    C,
}

/// <summary>Which code set, if any, a given ASCII character requires.</summary>
internal enum Code128CharacterRequirement
{
    /// <summary>The character is only encodable in Code Set A.</summary>
    RequiresA,

    /// <summary>The character is only encodable in Code Set B.</summary>
    RequiresB,

    /// <summary>The character is encodable in either Code Set A or Code Set B.</summary>
    EitherAOrB,
}
