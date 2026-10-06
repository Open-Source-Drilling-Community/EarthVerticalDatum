using OSDC.DotnetLibraries.Drilling.SemanticCatalogue;

namespace OSDC.Drilling.EarthVerticalDatum.Model;

/// <summary>An input WGS84 position and its corresponding EGM84 mean-sea-level depth.</summary>
[Semantic(Concepts.VerticalDatumSample)]
public class Wgs84ToMeanSeaLevelSample
{
    [Semantic(Concepts.Position)]
    public Wgs84ToMeanSeaLevelPosition Position { get; set; } = new();

    /// <summary>Depth in SI metres, positive downward from the EGM84 mean-sea-level geoid.</summary>
    [Semantic(Concepts.GeoidReferencedDepth, Reference = Concepts.Egm84Geoid)]
    public double MeanSeaLevelDepth { get; set; }

    /// <summary>EGM84 geoid undulation in SI metres, positive upward from the WGS84 ellipsoid to the geoid.</summary>
    [Semantic(Concepts.GeoidUndulation)]
    public double GeoidUndulation { get; set; }
}
