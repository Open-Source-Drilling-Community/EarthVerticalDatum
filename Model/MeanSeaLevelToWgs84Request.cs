using System.ComponentModel.DataAnnotations;

namespace OSDC.Drilling.EarthVerticalDatum.Model;

/// <summary>A stateless synchronous request to convert mean-sea-level depths to WGS84 ellipsoidal depths.</summary>
public class MeanSeaLevelToWgs84Request
{
    /// <summary>Positions and mean-sea-level depths to convert. The entire request is rejected when any item is invalid.</summary>
    [Required, MinLength(1)]
    public List<EarthVerticalDatumPosition> Positions { get; set; } = [];
}
