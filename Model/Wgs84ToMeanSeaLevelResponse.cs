namespace OSDC.Drilling.EarthVerticalDatum.Model;

/// <summary>EGM84-30 inverse-conversion results in the same order as the request positions.</summary>
public class Wgs84ToMeanSeaLevelResponse
{
    public EarthVerticalDatumModelInfo Model { get; set; } = new();
    public List<Wgs84ToMeanSeaLevelSample> Samples { get; set; } = [];
}
