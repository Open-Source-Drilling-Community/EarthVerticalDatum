using OSDC.DotnetLibraries.Drilling.SemanticCatalogue;
namespace OSDC.Drilling.EarthVerticalDatum.Model;

/// <summary>An input position and its corresponding WGS84 ellipsoidal-depth conversion.</summary>
[Semantic(Concepts.VerticalDatumSample)]
public class EarthVerticalDatumSample
{
    [Semantic(Concepts.GeoidDepthPosition)]
    public EarthVerticalDatumPosition Position { get; set; } = new();

    /// <summary>Depth in SI metres, positive downward from the WGS84 reference ellipsoid.</summary>
    [Semantic(Concepts.EllipsoidalDepth, Reference = Concepts.Wgs84)]
    public double Wgs84EllipsoidalDepth { get; set; }

    /// <summary>EGM84 geoid undulation in SI metres, positive upward from the WGS84 ellipsoid to the geoid.</summary>
    [Semantic(Concepts.GeoidUndulation)]
    public double GeoidUndulation { get; set; }
}
