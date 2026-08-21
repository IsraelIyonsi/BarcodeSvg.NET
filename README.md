# BarcodeSvg.NET

Code 128, EAN-13 and UPC-A barcodes rendered as self-contained SVG for .NET. No `System.Drawing`, no native imaging library, no runtime dependencies at all.

Most .NET barcode libraries either wrap `System.Drawing.Common` (Windows-only in practice, and Microsoft has been steering people away from it for years) or pull in a native imaging dependency to rasterize bitmaps. Neither belongs in a server-side PDF pipeline, an Azure Function, or anything you want to run on Linux or in a container. An SVG is just text: it drops straight into a browser, a PDF generator, or an email template, scales without artifacts, and needs nothing installed on the machine that renders it. BarcodeSvg.NET produces that text directly, with the two symbologies that cover the overwhelming majority of real-world barcoding: Code 128 for arbitrary alphanumeric data (invoice numbers, tracking codes, IDs) and EAN-13 for retail product barcodes.

Where the numbers matter, the library gets them right and proves it: the Code 128 check character (weighted modulo 103) and its automatic Code Set A/B/C switching, and the EAN-13 check digit (weighted modulo 10) and its left-hand odd/even parity pattern, are all verified against independently computed reference patterns, not just "it ran without throwing."

## Install

```
dotnet add package BarcodeSvg.Net
```

## Quickstart

```csharp
using BarcodeSvg;

string svg = Code128Barcode.ToSvg("INV-2026-004821");
```

That single call encodes the text and renders it straight to an SVG string, ready to embed in HTML, hand to a PDF library, or write to a file.

## EAN-13 with a computed check digit

```csharp
using BarcodeSvg;

// 12 digits in: the check digit is computed and appended automatically.
string svg = Ean13Barcode.ToSvg("400638133393");
```

Pass all 13 digits instead and the library validates the check digit for you, throwing a `FormatException` if it does not match, which catches typos before a mis-printed barcode ships.

## UPC-A

UPC-A is the North American retail barcode. It is structurally EAN-13 with an implicit leading number-system digit of `0`, so BarcodeSvg.NET encodes it by reusing the exact EAN-13 digit tables, guard bars and weighted modulo-10 checksum:

```csharp
using BarcodeSvg;

// 11 digits in: the check digit is computed and appended automatically.
string svg = UpcABarcode.ToSvg("03600029145");
```

Pass all 12 digits instead and the check digit is validated, throwing a `FormatException` on a mismatch. The human-readable line under the bars is the 12-digit UPC-A value.

## Sizing, color and the human-readable text line

```csharp
using BarcodeSvg;

var options = new BarcodeRenderOptions
{
    ModuleWidth = 2.5,
    Height = 60,
    QuietZoneWidth = 20,
    BarColor = "#111111",
    Text = "Invoice INV-2026-004821",
};

string svg = Code128Barcode.ToSvg("INV-2026-004821", options);
```

Set `ShowText = false` to omit the human-readable line entirely. Whatever text you supply (or the raw encoded value, if you do not override it) is XML-escaped before it reaches the SVG, so it cannot break the markup or inject content, even if it originated from user input.

## Working with the raw pattern

If you want to rasterize a barcode yourself, drive a thermal printer, or just inspect what was encoded, skip the SVG renderer and use the encoder directly:

```csharp
using BarcodeSvg;

BarcodePattern pattern = Code128Encoder.Encode("INV-2026-004821");

foreach (BarSegment segment in pattern.Bars)
{
    // segment.IsBar: true for a dark bar, false for a light space
    // segment.WidthInModules: how many module-widths wide this run is
}
```

`BarcodePattern` also exposes `TotalModules`, the symbology's `RecommendedQuietZoneModules`, and the default `HumanReadableText`, everything `BarcodeSvgRenderer.Render(pattern, options)` needs, and everything a custom renderer needs too.

## What is verified, and how

- **Code 128 checksum**: the weighted modulo-103 check character formula (`start value + Σ symbol value × position, mod 103`), verified against an independently written Python reference encoder and hand-checked arithmetic.
- **Code 128 code-set switching**: automatic Code Set A/B/C selection with LATCH for runs of two or more characters that need the other set, SHIFT for a single isolated character, and the exact digit-run threshold (four or more consecutive digits) that triggers Code Set C, including the odd-leftover-digit case where a run packs as many pairs as it can and falls back to A/B for the last digit.
- **EAN-13 checksum**: the weighted modulo-10 check digit, verified against the classic "4006381333931" worked example (a 375g box of Kellogg's Corn Flakes, the example used throughout the barcode literature) and several independently generated test values.
- **EAN-13 parity**: the left-hand odd/even (L/G) parity pattern for all ten possible leading digits, each verified against an exact, independently computed 95-module bit pattern.
- **UPC-A**: the "036000291452" worked example (check digit 2), with the encoded bars asserted byte-for-byte equal to the EAN-13 bars of `0` + the 12 digits, proving the symbology is reused rather than reimplemented.
- **SVG well-formedness and injection safety**: every rendered barcode is parsed back with `System.Xml.Linq.XDocument` in the test suite to confirm it is well-formed, and human-readable text and color options are fuzzed with `<script>` tags, quotes and ampersands to confirm they render as inert text, never as markup.
- **Determinism**: rendering the same pattern with the same options twice is asserted to produce byte-identical output.

## Dependencies and AOT

Zero runtime NuGet dependencies. No `System.Drawing`, no native interop, no reflection anywhere in the encoding or rendering path, just plain data-driven lookup tables and string building. The library is fully trimmer and Native AOT safe.

## License

MIT. See [LICENSE](LICENSE).
