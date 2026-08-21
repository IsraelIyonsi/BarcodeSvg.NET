# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [0.2.0] - 2026-08-21

### Added

- `UpcAEncoder.Encode(string)` and `UpcAEncoder.ComputeCheckDigit(ReadOnlySpan<char>)`: UPC-A (ISO/IEC 15420) encoding from 11 digits (check digit computed) or 12 digits (check digit validated). UPC-A is the EAN-13 symbol with an implicit leading number-system digit of `0`, so the encoder reuses the EAN-13 digit tables, guard bars, parity table and weighted modulo-10 checksum, and reports the human-readable line as the 12-digit UPC-A value.
- `UpcABarcode`: one-call convenience wrapper that encodes and renders a UPC-A symbol to SVG in a single method.
- Verified that a UPC-A symbol's bars are byte-for-byte the EAN-13 bars of `0` + the 12 digits (proving the reuse), against the classic "036000291452" worked example whose check digit is 2.

## [0.1.0] - 2026-08-12

### Added

- `Code128Encoder.Encode(string)`: Code 128 encoding with automatic Code Set A/B/C selection and optimal switching (LATCH for runs of deviating characters, SHIFT for a single isolated one, Code Set C for digit runs of four or more), producing a `BarcodePattern`.
- `Ean13Encoder.Encode(string)` and `Ean13Encoder.ComputeCheckDigit(ReadOnlySpan<char>)`: EAN-13 encoding from 12 digits (check digit computed) or 13 digits (check digit validated), with the correct left-hand odd/even parity pattern driven by the leading digit.
- `BarcodePattern` and `BarSegment`: the symbology-agnostic bar/space run-length pattern, exposed so callers can render or rasterize a barcode themselves instead of using the bundled SVG renderer.
- `BarcodeSvgRenderer.Render(BarcodePattern, BarcodeRenderOptions)`: renders a pattern to a self-contained, well-formed SVG string with no external references. Deterministic byte-for-byte for a given pattern and options.
- `BarcodeRenderOptions`: width, module width, height, quiet zone width, bar and background color, font, and an optional human-readable text line. Human-readable text and color values are XML-escaped, so untrusted input cannot inject markup into the rendered SVG.
- `Code128Barcode` and `Ean13Barcode`: one-call convenience wrappers that encode and render to SVG in a single method.
- Verified against independently computed reference patterns: the Code 128 checksum and code-set switching against an independently written Python reference encoder, and EAN-13 against the classic "4006381333931" worked example plus a fixture covering every possible leading-digit parity pattern.
- Zero runtime dependencies; no `System.Drawing`, no native interop, no reflection. Trimmer and Native AOT safe.
