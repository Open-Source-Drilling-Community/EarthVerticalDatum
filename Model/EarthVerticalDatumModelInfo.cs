namespace OSDC.Drilling.EarthVerticalDatum.Model;

/// <summary>Identity, accuracy, and provenance of the geoid model used for conversion.</summary>
public class EarthVerticalDatumModelInfo
{
    public string Name { get; set; } = string.Empty;
    public string ID { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime? DataDateTime { get; set; }
    public double GridResolutionMinutes { get; set; }
    public string Interpolation { get; set; } = string.Empty;
    public double MaximumInterpolationError { get; set; }
    public double RMSInterpolationError { get; set; }
    public string GeographicLibVersion { get; set; } = string.Empty;
    public string ReferenceEllipsoid { get; set; } = "WGS84";
    public string SourceVerticalDatum { get; set; } = "EGM84 mean-sea-level geoid";
    public string TargetVerticalDatum { get; set; } = "WGS84 reference ellipsoid";
    public string DepthPositiveDirection { get; set; } = "down";
    public bool IsThreadSafe { get; set; }
    public string CoefficientSHA256 { get; set; } = string.Empty;
}
