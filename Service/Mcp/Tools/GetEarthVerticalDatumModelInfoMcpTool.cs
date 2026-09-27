using OSDC.DotnetLibraries.Drilling.SemanticCatalogue;
using System.Text.Json;
using System.Text.Json.Nodes;
using OSDC.Drilling.EarthVerticalDatum.Model;

namespace OSDC.Drilling.EarthVerticalDatum.Service.Mcp.Tools;

public sealed class GetEarthVerticalDatumModelInfoMcpTool(EarthVerticalDatumEvaluator evaluator) : IMcpTool
{
    public string Name => "earth_vertical_datum_get_model_info";
    public string Description => "Returns the loaded EGM84-30 geoid model identity and provenance, including its 30-arcminute grid spacing (AngularGridSpacing in SI radians), explicit cubic interpolation, published interpolation-error estimates, data timestamp, GeographicLib runtime version, WGS84 reference ellipsoid, positive-down API convention, thread-safety mode, and coefficient-file SHA-256. Use it for traceability and deployment comparison. It performs no conversion and persists nothing.";
    public JsonNode InputSchema { get; } = JsonNode.Parse("""{"type":"object","properties":{},"additionalProperties":false}""")!;
    public JsonNode OutputSchema { get; } = CreateOutputSchema();
    internal static JsonNode CreateOutputSchema()
    {
        JsonNode schema = BuildOutputSchema();
        SemanticMetadata.AnnotateObject((JsonObject)schema, typeof(EarthVerticalDatumModelInfo));
        return schema;
    }
    private static JsonNode BuildOutputSchema() => JsonNode.Parse("""
    {
      "type": "object",
      "description": "Identity, accuracy, and reproducibility metadata for the loaded geoid model.",
      "properties": {
        "Name": { "type": "string", "description": "GeographicLib geoid-grid name." },
        "ID": { "type": "string", "const": "EGM84-30", "description": "Identifier of the installed model and grid realization." },
        "Description": { "type": "string", "description": "Description embedded in the grid file." },
        "DataDateTime": { "type": ["string", "null"], "format": "date-time", "description": "Dataset timestamp from the grid header, marked UTC; not the conversion time or necessarily a publication date." },
        "AngularGridSpacing": { "type": "number", "const": 0.008726646259971648, "description": "Angular spacing between grid nodes in SI radians along latitude and longitude; pi/360 for this grid (30 arcminutes)." },
        "Interpolation": { "type": "string", "description": "Interpolation method used to evaluate the sampled geoid." },
        "MaximumInterpolationError": { "type": "number", "description": "Estimated maximum absolute representation error in SI metres relative to the reference geoid, including quantization and interpolation; not total geoid-model accuracy." },
        "RMSInterpolationError": { "type": "number", "description": "Estimated root mean square representation error in SI metres relative to the reference geoid, including quantization and interpolation; not a standard deviation or total geoid-model accuracy." },
        "GeographicLibVersion": { "type": "string", "description": "Version of the calculation implementation." },
        "ReferenceEllipsoid": { "description": "Ellipsoid used for geodetic coordinates and ellipsoidal depth.", "type": "string", "const": "WGS84" },
        "SupportedVerticalDatums": { "type": "array", "items": { "type": "string" }, "minItems": 2, "description": "Supported vertical reference surfaces; EGM84 is a model geoid, not a local tidal datum or instantaneous sea surface." },
        "SupportedConversionDirections": { "type": "array", "items": { "type": "string" }, "minItems": 2, "description": "Supported source-to-target reference-surface pairs." },
        "DepthPositiveDirection": { "type": "string", "const": "down", "description": "Public depth coordinates increase downward from their named reference surfaces." },
        "IsThreadSafe": { "type": "boolean", "const": true, "description": "Whether the loaded model supports concurrent evaluations." },
        "CoefficientSHA256": { "type": "string", "pattern": "^[0-9a-fA-F]{64}$", "description": "SHA-256 digest of the complete PGM grid file bytes, encoded as 64 lowercase hexadecimal characters." }
      },
      "required": ["Name", "ID", "Description", "DataDateTime", "AngularGridSpacing", "Interpolation", "MaximumInterpolationError", "RMSInterpolationError", "GeographicLibVersion", "ReferenceEllipsoid", "SupportedVerticalDatums", "SupportedConversionDirections", "DepthPositiveDirection", "IsThreadSafe", "CoefficientSHA256"],
      "additionalProperties": false
    }
    """)!;

    public Task<JsonNode?> InvokeAsync(JsonObject? arguments, CancellationToken cancellationToken) =>
        Task.FromResult(JsonSerializer.SerializeToNode(evaluator.ModelInfo));
}
