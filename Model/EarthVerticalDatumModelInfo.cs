using OSDC.DotnetLibraries.Drilling.SemanticCatalogue;

namespace OSDC.Drilling.EarthVerticalDatum.Model;

/// <summary>Identity, accuracy, and provenance of the geoid model used for conversion.</summary>
[Semantic(Concepts.GeoidModelProvenance)]
public class EarthVerticalDatumModelInfo
{
    [Semantic(Concepts.ModelName)]
    public string Name { get; set; } = string.Empty;
    [Semantic(Concepts.ModelId)]
    public string ID { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    [Semantic(Concepts.Instant, Role = Concepts.DatasetTimestamp, Reference = Concepts.Utc)]
    public DateTime? DataDateTime { get; set; }
    [Semantic(Concepts.AngularGridSpacing)]
    public double AngularGridSpacing { get; set; }
    [Semantic(Concepts.InterpolationMethod)]
    public string Interpolation { get; set; } = string.Empty;
    [Semantic(Concepts.GeoidApproximationError, Role = Concepts.MaximumAbsoluteError)]
    public double MaximumInterpolationError { get; set; }
    [Semantic(Concepts.GeoidApproximationError, Role = Concepts.RootMeanSquareError)]
    public double RMSInterpolationError { get; set; }
    [Semantic(Concepts.RuntimeVersion)]
    public string GeographicLibVersion { get; set; } = string.Empty;
    [Semantic(Concepts.ReferenceEllipsoid)]
    public string ReferenceEllipsoid { get; set; } = "WGS84";
    public List<string> SupportedVerticalDatums { get; set; } =
        ["EGM84 mean-sea-level geoid", "WGS84 reference ellipsoid"];
    public List<string> SupportedConversionDirections { get; set; } =
        ["EGM84 mean-sea-level geoid to WGS84 reference ellipsoid", "WGS84 reference ellipsoid to EGM84 mean-sea-level geoid"];
    public string DepthPositiveDirection { get; set; } = "down";
    public bool IsThreadSafe { get; set; }
    [Semantic(Concepts.Sha256FileDigest, Role = Concepts.CoefficientFile)]
    public string CoefficientSHA256 { get; set; } = string.Empty;
}
