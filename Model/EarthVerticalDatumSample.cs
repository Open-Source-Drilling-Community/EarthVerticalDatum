namespace OSDC.Drilling.EarthVerticalDatum.Model;

/// <summary>An input position and its corresponding WGS84 ellipsoidal-depth conversion.</summary>
public class EarthVerticalDatumSample
{
    public EarthVerticalDatumPosition Position { get; set; } = new();

    /// <summary>Depth in SI metres, positive downward from the WGS84 reference ellipsoid.</summary>
    public double Wgs84EllipsoidalDepth { get; set; }

    /// <summary>EGM84 geoid undulation in SI metres, positive upward from the WGS84 ellipsoid to the geoid.</summary>
    public double GeoidUndulation { get; set; }
}
