namespace BarcodeSvg;

/// <summary>
/// Encodes text into a Code 128 (ISO/IEC 15417) bar/space pattern, automatically choosing and
/// switching between Code Set A, B and C to minimize the number of symbol characters.
/// </summary>
public static class Code128Encoder
{
    /// <summary>
    /// The Code 128 quiet zone width recommended by the specification, in modules, applied on
    /// each side of the symbol when no explicit override is given to the renderer.
    /// </summary>
    internal const int RecommendedQuietZoneModules = 10;

    /// <summary>
    /// Encodes <paramref name="data"/> as a Code 128 symbol.
    /// </summary>
    /// <param name="data">
    /// The text to encode. Every character must be in the ASCII range 0-127; Code 128's optional
    /// FNC4 extension for Latin-1 characters above 127 is not supported.
    /// </param>
    /// <returns>The resulting bar/space pattern.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="data"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="data"/> is empty, or contains a character above U+007F.
    /// </exception>
    public static BarcodePattern Encode(string data)
    {
        ArgumentNullException.ThrowIfNull(data);
        if (data.Length == 0)
        {
            throw new ArgumentException("Code 128 data must not be empty.", nameof(data));
        }

        foreach (var c in data)
        {
            if (c > Code128Symbology.MaxSupportedAsciiValue)
            {
                throw new ArgumentException(
                    $"Character U+{(int)c:X4} is outside the ASCII range Code 128 supports without FNC4 extensions.",
                    nameof(data));
            }
        }

        var symbolValues = BuildSymbolValues(data);
        var bars = BuildBarSegments(symbolValues);
        return new BarcodePattern(bars, RecommendedQuietZoneModules, data);
    }

    private static List<int> BuildSymbolValues(string data)
    {
        var values = new List<int>(data.Length + 3);
        var current = ChooseStartCodeSet(data);
        values.Add(StartValueFor(current));

        var index = 0;
        while (index < data.Length)
        {
            if (current == Code128CodeSet.C)
            {
                var digitRun = CountConsecutiveDigits(data, index);
                if (digitRun >= Code128Symbology.CodeCDigitsPerSymbol)
                {
                    var high = data[index] - '0';
                    var low = data[index + 1] - '0';
                    values.Add((high * 10) + low);
                    index += Code128Symbology.CodeCDigitsPerSymbol;
                    continue;
                }

                var target = RequirementFor(data[index]) == Code128CharacterRequirement.RequiresA
                    ? Code128CodeSet.A
                    : Code128CodeSet.B;
                values.Add(LatchValueFor(target));
                current = target;
                continue;
            }

            var digitRunInAOrB = CountConsecutiveDigits(data, index);
            if (digitRunInAOrB >= Code128Symbology.MinimumCodeCDigitRun)
            {
                values.Add(Code128Symbology.CodeSetCValue);
                current = Code128CodeSet.C;
                continue;
            }

            var requirement = RequirementFor(data[index]);
            var satisfiesCurrent = requirement == Code128CharacterRequirement.EitherAOrB
                || (requirement == Code128CharacterRequirement.RequiresA && current == Code128CodeSet.A)
                || (requirement == Code128CharacterRequirement.RequiresB && current == Code128CodeSet.B);

            if (satisfiesCurrent)
            {
                values.Add(SymbolValueFor(current, data[index]));
                index++;
                continue;
            }

            var deviatingSet = requirement == Code128CharacterRequirement.RequiresA ? Code128CodeSet.A : Code128CodeSet.B;
            var nextAlsoDeviates = index + 1 < data.Length && RequirementFor(data[index + 1]) == requirement;
            if (nextAlsoDeviates)
            {
                values.Add(LatchValueFor(deviatingSet));
                current = deviatingSet;
            }
            else
            {
                values.Add(Code128Symbology.ShiftValue);
                values.Add(SymbolValueFor(deviatingSet, data[index]));
                index++;
            }
        }

        values.Add(ComputeCheckValue(values));
        values.Add(Code128Symbology.StopValue);
        return values;
    }

    private static Code128CodeSet ChooseStartCodeSet(string data)
    {
        if (CountConsecutiveDigits(data, 0) >= Code128Symbology.MinimumCodeCDigitRun)
        {
            return Code128CodeSet.C;
        }

        return RequirementFor(data[0]) == Code128CharacterRequirement.RequiresA
            ? Code128CodeSet.A
            : Code128CodeSet.B;
    }

    private static Code128CharacterRequirement RequirementFor(char c)
    {
        if (c < Code128Symbology.ControlCharacterUpperBoundExclusive)
        {
            return Code128CharacterRequirement.RequiresA;
        }

        return c <= Code128Symbology.SharedCharacterUpperBoundInclusive
            ? Code128CharacterRequirement.EitherAOrB
            : Code128CharacterRequirement.RequiresB;
    }

    private static int SymbolValueFor(Code128CodeSet set, char c) => set switch
    {
        Code128CodeSet.A => c < Code128Symbology.ControlCharacterUpperBoundExclusive
            ? c + Code128Symbology.CodeSetAControlOffset
            : c - Code128Symbology.CodeSetBAsciiOffset,
        Code128CodeSet.B => c - Code128Symbology.CodeSetBAsciiOffset,
        _ => throw new InvalidOperationException("Code Set C characters are encoded as digit pairs, not single symbol values."),
    };

    private static int StartValueFor(Code128CodeSet set) => set switch
    {
        Code128CodeSet.A => Code128Symbology.StartAValue,
        Code128CodeSet.B => Code128Symbology.StartBValue,
        Code128CodeSet.C => Code128Symbology.StartCValue,
        _ => throw new ArgumentOutOfRangeException(nameof(set)),
    };

    private static int LatchValueFor(Code128CodeSet set) => set switch
    {
        Code128CodeSet.A => Code128Symbology.CodeSetAValue,
        Code128CodeSet.B => Code128Symbology.CodeSetBValue,
        Code128CodeSet.C => Code128Symbology.CodeSetCValue,
        _ => throw new ArgumentOutOfRangeException(nameof(set)),
    };

    private static int CountConsecutiveDigits(string data, int start)
    {
        var count = 0;
        while (start + count < data.Length && char.IsAsciiDigit(data[start + count]))
        {
            count++;
        }

        return count;
    }

    private static int ComputeCheckValue(List<int> values)
    {
        var checksum = values[0];
        for (var i = 1; i < values.Count; i++)
        {
            checksum += values[i] * i;
        }

        return checksum % Code128Symbology.ChecksumModulus;
    }

    private static List<BarSegment> BuildBarSegments(List<int> values)
    {
        var bars = new List<BarSegment>(values.Count * Code128Symbology.SymbolWidthInModules);
        var isBar = true;
        foreach (var value in values)
        {
            var pattern = value == Code128Symbology.StopValue
                ? Code128Symbology.StopPattern
                : Code128Symbology.Patterns[value];

            foreach (var width in pattern)
            {
                bars.Add(new BarSegment(isBar, width));
                isBar = !isBar;
            }
        }

        return bars;
    }
}
