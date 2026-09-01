namespace OSDC.Drilling.EarthVerticalDatum.Service;

public sealed class EarthVerticalDatumServiceOptions
{
    public const string SectionName = "EarthVerticalDatum";
    public int MaximumPositionsPerRequest { get; set; } = 10_000;
    public string? ModelDirectory { get; set; }
    public string UsageStatisticsFile { get; set; } = "home/EarthVerticalDatum.UsageStatistics.json";
    public int UsageStatisticsSaveIntervalSeconds { get; set; } = 30;
}
