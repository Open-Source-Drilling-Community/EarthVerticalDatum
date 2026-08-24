using System.ComponentModel.DataAnnotations;

namespace OSDC.Drilling.EarthVerticalDatum.Model;

/// <summary>A WGS84 horizontal position and a mean-sea-level depth expressed using OSDC SI conventions.</summary>
public class EarthVerticalDatumPosition
{
    /// <summary>WGS84 geodetic latitude in SI radians, between -π/2 and π/2.</summary>
    [Range(-1.5707963267948966, 1.5707963267948966)]
    public double Latitude { get; set; }

    /// <summary>WGS84 geodetic longitude in SI radians, between -π and π.</summary>
    [Range(-3.141592653589793, 3.141592653589793)]
    public double Longitude { get; set; }

    /// <summary>Depth in SI metres, positive downward from the EGM84 mean-sea-level geoid. A negative value is above that surface.</summary>
    public double MeanSeaLevelDepth { get; set; }
}
