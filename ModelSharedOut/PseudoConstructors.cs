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

    public static Wgs84ToMeanSeaLevelPosition ConstructWgs84ToMeanSeaLevelPosition() => new()
    {
        Latitude = 0,
        Longitude = 0,
        Wgs84EllipsoidalDepth = 0
    };

    public static Wgs84ToMeanSeaLevelRequest ConstructWgs84ToMeanSeaLevelRequest() => new()
    {
        Positions = [ConstructWgs84ToMeanSeaLevelPosition()]
    };

}
