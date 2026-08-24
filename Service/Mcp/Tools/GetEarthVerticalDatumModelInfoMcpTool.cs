using System.Text.Json;
using System.Text.Json.Nodes;
using OSDC.Drilling.EarthVerticalDatum.Model;

namespace OSDC.Drilling.EarthVerticalDatum.Service.Mcp.Tools;

public sealed class GetEarthVerticalDatumModelInfoMcpTool(EarthVerticalDatumEvaluator evaluator) : IMcpTool
{
    public string Name => "earth_vertical_datum_get_model_info";
    public string Description => "Returns the loaded EGM84-30 geoid model identity and provenance, including its 30-minute grid resolution, explicit cubic interpolation, published interpolation-error estimates, data timestamp, GeographicLib runtime version, WGS84 reference ellipsoid, positive-down API convention, thread-safety mode, and coefficient-file SHA-256. Use it for traceability and deployment comparison. It performs no conversion and persists nothing.";
    public JsonNode InputSchema { get; } = JsonNode.Parse("""{"type":"object","properties":{},"additionalProperties":false}""")!;
    public JsonNode OutputSchema { get; } = JsonNode.Parse("""
    {
      "type": "object",
      "description": "Identity, accuracy, and reproducibility metadata for the loaded geoid model.",
      "properties": {
        "Name": { "type": "string", "description": "GeographicLib geoid-grid name." },
        "ID": { "type": "string", "const": "EGM84-30", "description": "Stable service identifier for the model and grid resolution." },
        "Description": { "type": "string", "description": "Description embedded in the GeographicLib grid file." },
        "DataDateTime": { "type": ["string", "null"], "format": "date-time", "description": "Dataset timestamp embedded in the grid file." },
        "GridResolutionMinutes": { "type": "number", "const": 30, "description": "Angular grid spacing in arc minutes." },
        "Interpolation": { "type": "string", "description": "Interpolation method explicitly selected when loading the model." },
        "MaximumInterpolationError": { "type": "number", "description": "Published maximum interpolation error in metres for the selected method." },
        "RMSInterpolationError": { "type": "number", "description": "Published RMS interpolation error in metres for the selected method." },
        "GeographicLibVersion": { "type": "string", "description": "GeographicLib runtime version." },
        "ReferenceEllipsoid": { "type": "string", "const": "WGS84" },
        "SupportedVerticalDatums": { "type": "array", "items": { "type": "string" }, "minItems": 2, "description": "Vertical reference surfaces supported by the service." },
        "SupportedConversionDirections": { "type": "array", "items": { "type": "string" }, "minItems": 2, "description": "Both supported conversion directions, expressed without implying a permanent source or target." },
        "DepthPositiveDirection": { "type": "string", "const": "down", "description": "Sign convention of public depth properties." },
        "IsThreadSafe": { "type": "boolean", "const": true, "description": "Whether the in-memory model supports concurrent requests." },
        "CoefficientSHA256": { "type": "string", "pattern": "^[0-9a-fA-F]{64}$", "description": "SHA-256 of the installed PGM grid file." }
      },
      "required": ["Name", "ID", "Description", "DataDateTime", "GridResolutionMinutes", "Interpolation", "MaximumInterpolationError", "RMSInterpolationError", "GeographicLibVersion", "ReferenceEllipsoid", "SupportedVerticalDatums", "SupportedConversionDirections", "DepthPositiveDirection", "IsThreadSafe", "CoefficientSHA256"],
      "additionalProperties": false
    }
    """)!;

    public Task<JsonNode?> InvokeAsync(JsonObject? arguments, CancellationToken cancellationToken) =>
        Task.FromResult(JsonSerializer.SerializeToNode(evaluator.ModelInfo));
}
