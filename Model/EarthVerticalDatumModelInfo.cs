using OSDC.DotnetLibraries.Drilling.SemanticCatalogue;
namespace OSDC.Drilling.EarthVerticalDatum.Model;

/// <summary>Identity, accuracy, and provenance of the geoid model used for conversion.</summary>
[Semantic(Concepts.GeoidModelProvenance)]
public class EarthVerticalDatumModelInfo
{
    /// <summary>GeographicLib geoid-grid name.</summary>
    [Semantic(Concepts.ModelName)]
    public string Name { get; set; } = string.Empty;
    /// <summary>Identifier of the installed model and grid realization.</summary>
    [Semantic(Concepts.ModelId)]
    public string ID { get; set; } = string.Empty;
    /// <summary>Description embedded in the grid file.</summary>
    public string Description { get; set; } = string.Empty;
    /// <summary>Dataset timestamp from the grid header, marked UTC; not the conversion time or necessarily a publication date.</summary>
    [Semantic(Concepts.Instant, Role = Concepts.DatasetTimestamp, Reference = Concepts.Utc)]
    public DateTime? DataDateTime { get; set; }
    /// <summary>Angular spacing between grid nodes in SI radians along latitude and longitude; pi/360 for this grid (30 arcminutes).</summary>
    [Semantic(Concepts.AngularGridSpacing)]
    public double AngularGridSpacing { get; set; }
    /// <summary>Interpolation method used to evaluate the sampled geoid.</summary>
    [Semantic(Concepts.InterpolationMethod)]
    public string Interpolation { get; set; } = string.Empty;
    /// <summary>Estimated maximum absolute representation error in SI metres relative to the reference geoid, including quantization and interpolation; not total geoid-model accuracy.</summary>
    [Semantic(Concepts.GeoidApproximationError, Role = Concepts.MaximumAbsoluteError)]
    public double MaximumInterpolationError { get; set; }
    /// <summary>Estimated root mean square representation error in SI metres relative to the reference geoid, including quantization and interpolation; not a standard deviation or total geoid-model accuracy.</summary>
    [Semantic(Concepts.GeoidApproximationError, Role = Concepts.RootMeanSquareError)]
    public double RMSInterpolationError { get; set; }
    /// <summary>Version of the calculation implementation.</summary>
    [Semantic(Concepts.RuntimeVersion)]
    public string GeographicLibVersion { get; set; } = string.Empty;
    /// <summary>Ellipsoid used for geodetic coordinates and ellipsoidal depth.</summary>
    [Semantic(Concepts.ReferenceEllipsoid)]
    public string ReferenceEllipsoid { get; set; } = "WGS84";
    /// <summary>Supported vertical reference surfaces; EGM84 is a model geoid, not a local tidal datum or instantaneous sea surface.</summary>
    public List<string> SupportedVerticalDatums { get; set; } =
        ["EGM84 mean-sea-level geoid", "WGS84 reference ellipsoid"];
    /// <summary>Supported source-to-target reference-surface pairs.</summary>
    public List<string> SupportedConversionDirections { get; set; } =
        ["EGM84 mean-sea-level geoid to WGS84 reference ellipsoid", "WGS84 reference ellipsoid to EGM84 mean-sea-level geoid"];
    /// <summary>Public depth coordinates increase downward from their named reference surfaces.</summary>
    public string DepthPositiveDirection { get; set; } = "down";
    /// <summary>Whether the loaded model supports concurrent evaluations.</summary>
    public bool IsThreadSafe { get; set; }
    /// <summary>SHA-256 digest of the complete PGM grid file bytes, encoded as 64 lowercase hexadecimal characters.</summary>
    [Semantic(Concepts.Sha256FileDigest, Role = Concepts.CoefficientFile)]
    public string CoefficientSHA256 { get; set; } = string.Empty;
}
