using System.ComponentModel.DataAnnotations;

namespace OSDC.Drilling.EarthVerticalDatum.Model;

/// <summary>A stateless synchronous request to convert WGS84 ellipsoidal depths to mean-sea-level depths.</summary>
public class Wgs84ToMeanSeaLevelRequest
{
    /// <summary>Positions and WGS84 ellipsoidal depths to convert. Any invalid item rejects the entire request.</summary>
    [Required, MinLength(1)]
    public List<Wgs84ToMeanSeaLevelPosition> Positions { get; set; } = [];
}
