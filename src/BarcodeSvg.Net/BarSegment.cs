namespace BarcodeSvg;

/// <summary>
/// A single run of consecutive same-polarity modules in a 1D barcode: either a dark bar or a
/// light space, expressed as a whole number of modules wide.
/// </summary>
/// <param name="IsBar">
/// <see langword="true"/> when this run is a dark, ink-printed bar; <see langword="false"/> when
/// it is a light space.
/// </param>
/// <param name="WidthInModules">
/// The width of this run, measured in modules (the narrowest element width of the symbology).
/// Always a positive integer.
/// </param>
public readonly record struct BarSegment(bool IsBar, int WidthInModules);
