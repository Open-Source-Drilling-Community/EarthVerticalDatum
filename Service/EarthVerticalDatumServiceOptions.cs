namespace OSDC.Drilling.EarthVerticalDatum.Service;

public sealed class EarthVerticalDatumServiceOptions
{
    public const string SectionName = "EarthVerticalDatum";
    public int MaximumPositionsPerRequest { get; set; } = 10_000;
    public string? ModelDirectory { get; set; }
}
