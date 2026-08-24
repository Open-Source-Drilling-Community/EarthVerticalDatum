namespace OSDC.Drilling.EarthVerticalDatum.Model;

/// <summary>An input WGS84 position and its corresponding EGM84 mean-sea-level depth.</summary>
public class Wgs84ToMeanSeaLevelSample
{
    public Wgs84ToMeanSeaLevelPosition Position { get; set; } = new();

    /// <summary>Depth in SI metres, positive downward from the EGM84 mean-sea-level geoid.</summary>
    public double MeanSeaLevelDepth { get; set; }

    /// <summary>EGM84 geoid undulation in SI metres, positive upward from the WGS84 ellipsoid to the geoid.</summary>
    public double GeoidUndulation { get; set; }
}
