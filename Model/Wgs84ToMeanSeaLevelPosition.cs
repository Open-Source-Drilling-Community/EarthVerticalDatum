using OSDC.DotnetLibraries.Drilling.SemanticCatalogue;
using System.ComponentModel.DataAnnotations;

namespace OSDC.Drilling.EarthVerticalDatum.Model;

/// <summary>A WGS84 horizontal position and ellipsoidal depth expressed using OSDC SI conventions.</summary>
[Semantic(Concepts.Position)]
public class Wgs84ToMeanSeaLevelPosition
{
    /// <summary>WGS84 geodetic latitude in SI radians, between -π/2 and π/2.</summary>
    [Range(-1.5707963267948966, 1.5707963267948966)]
    [Semantic(Concepts.Latitude, Reference = Concepts.Wgs84)]
    public double Latitude { get; set; }

    /// <summary>WGS84 geodetic longitude in SI radians, between -π and π.</summary>
    [Range(-3.141592653589793, 3.141592653589793)]
    [Semantic(Concepts.Longitude, Reference = Concepts.Wgs84)]
    public double Longitude { get; set; }

    /// <summary>Depth in SI metres, positive downward from the WGS84 reference ellipsoid. A negative value is above it.</summary>
    [Semantic(Concepts.EllipsoidalDepth, Reference = Concepts.Wgs84)]
    public double Wgs84EllipsoidalDepth { get; set; }
}
