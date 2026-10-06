using OSDC.DotnetLibraries.Drilling.SemanticCatalogue;

namespace OSDC.Drilling.EarthVerticalDatum.Model;

/// <summary>EGM84-30 inverse-conversion results in the same order as the request positions.</summary>
[Semantic(Concepts.VerticalDatumResponse)]
public class Wgs84ToMeanSeaLevelResponse
{
    [Semantic(Concepts.GeoidModelProvenance, Role = Concepts.Provenance)]
    public EarthVerticalDatumModelInfo Model { get; set; } = new();
    [Semantic(Concepts.VerticalDatumSample, Role = Concepts.OutputSamples)]
    public List<Wgs84ToMeanSeaLevelSample> Samples { get; set; } = [];
}
