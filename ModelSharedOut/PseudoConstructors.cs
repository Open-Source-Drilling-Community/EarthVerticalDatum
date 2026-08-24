namespace OSDC.Drilling.EarthVerticalDatum.ModelShared;

/// <summary>Convenience constructors for the generated stateless Earth Vertical Datum DTOs.</summary>
public static class PseudoConstructors
{
    public static EarthVerticalDatumPosition ConstructEarthVerticalDatumPosition() => new()
    {
        Latitude = 0,
        Longitude = 0,
        MeanSeaLevelDepth = 0
    };

    public static MeanSeaLevelToWgs84Request ConstructMeanSeaLevelToWgs84Request() => new()
    {
        Positions = [ConstructEarthVerticalDatumPosition()]
    };
}
