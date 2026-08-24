namespace OSDC.Drilling.EarthVerticalDatum.Model;

/// <summary>EGM84-30 conversion results in the same order as the request positions.</summary>
public class MeanSeaLevelToWgs84Response
{
    public EarthVerticalDatumModelInfo Model { get; set; } = new();
    public List<EarthVerticalDatumSample> Samples { get; set; } = [];
}
