using OSDC.DotnetLibraries.Drilling.SemanticCatalogue;
using System.ComponentModel.DataAnnotations;

namespace OSDC.Drilling.EarthVerticalDatum.Model;

/// <summary>A stateless synchronous request to convert mean-sea-level depths to WGS84 ellipsoidal depths.</summary>
[Semantic(Concepts.VerticalDatumRequest)]
public class MeanSeaLevelToWgs84Request
{
    /// <summary>Positions and mean-sea-level depths to convert. The entire request is rejected when any item is invalid.</summary>
    [Required, MinLength(1)]
    [Semantic(Concepts.GeoidDepthPosition, Role = Concepts.InputPositions)]
    public List<EarthVerticalDatumPosition> Positions { get; set; } = [];
}
