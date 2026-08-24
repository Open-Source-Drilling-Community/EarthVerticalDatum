using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Options;
using OSDC.Drilling.EarthVerticalDatum.Model;

namespace OSDC.Drilling.EarthVerticalDatum.Service.Mcp.Tools;

public sealed class ConvertWgs84ToMeanSeaLevelMcpTool : IMcpTool
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = null,
        PropertyNameCaseInsensitive = true
    };
    private readonly EarthVerticalDatumEvaluator evaluator_;
    private readonly UsageStatisticsEarthVerticalDatum statistics_;
    private readonly int maximumPositions_;

    public ConvertWgs84ToMeanSeaLevelMcpTool(EarthVerticalDatumEvaluator evaluator,
        UsageStatisticsEarthVerticalDatum statistics, IOptions<EarthVerticalDatumServiceOptions> options)
    {
        evaluator_ = evaluator;
        statistics_ = statistics;
        maximumPositions_ = options.Value.MaximumPositionsPerRequest;
        InputSchema = CreateInputSchema(maximumPositions_);
    }

    public string Name => "earth_vertical_datum_convert_wgs84_to_mean_sea_level";
    public string Description => "Synchronously converts one or more depths from the WGS84 reference ellipsoid to the EGM84 mean-sea-level geoid using the EGM84 30-minute grid with cubic interpolation. This is the inverse of earth_vertical_datum_convert_mean_sea_level_to_wgs84 and is stateless: results are returned directly, with no GUID, calculation order, dataset, or persistence. Latitude and Longitude MUST be WGS84 SI radians. Wgs84EllipsoidalDepth and MeanSeaLevelDepth are SI metres and positive downward; negative values are above their named reference surfaces. GeographicLib uses degrees and positive-up heights internally, but those conversions occur only at the library boundary. Samples preserve input order. GeoidUndulation is SI metres positive upward and satisfies MeanSeaLevelDepth = Wgs84EllipsoidalDepth + GeoidUndulation. Validation is atomic: one invalid position rejects the complete request with isError=true, no partial result, and structuredContent shaped as {Error, Message, Errors:[{PositionIndex, Property, Code, Message}]}; PositionIndex is zero-based for an item and null for a request-level error.";
    public JsonNode InputSchema { get; }
    public JsonNode OutputSchema { get; } = CreateOutputSchema();

    public Task<JsonNode?> InvokeAsync(JsonObject? arguments, CancellationToken cancellationToken)
    {
        Wgs84ToMeanSeaLevelRequest request = arguments?.Deserialize<Wgs84ToMeanSeaLevelRequest>(JsonOptions)
            ?? throw new ArgumentException("An object containing Positions is required.");
        statistics_.IncrementConversion(true, request.Positions?.Count ?? 0);
        try
        {
            Wgs84ToMeanSeaLevelResponse result = evaluator_.ConvertWgs84ToMeanSeaLevel(
                request, maximumPositions_, cancellationToken);
            return Task.FromResult(JsonSerializer.SerializeToNode(result, JsonOptions));
        }
        catch
        {
            statistics_.IncrementFailedConversion();
            throw;
        }
    }

    private static JsonNode CreateInputSchema(int maximumPositions) => JsonNode.Parse($$"""
    {
      "type": "object",
      "description": "Stateless synchronous WGS84 ellipsoidal-depth to EGM84 mean-sea-level depth conversion.",
      "properties": {
        "Positions": {
          "type": "array",
          "description": "Positions converted in order. Any invalid item rejects the complete request.",
          "minItems": 1,
          "maxItems": {{maximumPositions}},
          "items": {
            "type": "object",
            "properties": {
              "Latitude": { "type": "number", "minimum": -1.5707963267948966, "maximum": 1.5707963267948966, "description": "WGS84 geodetic latitude in SI radians. Do not supply degrees." },
              "Longitude": { "type": "number", "minimum": -3.141592653589793, "maximum": 3.141592653589793, "description": "WGS84 longitude in SI radians. Do not supply degrees." },
              "Wgs84EllipsoidalDepth": { "type": "number", "description": "Depth in SI metres, positive downward from the WGS84 reference ellipsoid; negative above it." }
            },
            "required": ["Latitude", "Longitude", "Wgs84EllipsoidalDepth"],
            "additionalProperties": false
          }
        }
      },
      "required": ["Positions"],
      "examples": [{ "Positions": [{ "Latitude": 0.8726646259971648, "Longitude": 0.5235987755982988, "Wgs84EllipsoidalDepth": 23.0 }] }],
      "additionalProperties": false
    }
    """)!;

    private static JsonNode CreateOutputSchema() => JsonNode.Parse("""
    {
      "type": "object",
      "description": "Successful inverse conversion with samples in the same order as the request.",
      "properties": {
        "Model": { "$ref": "#/$defs/modelInfo" },
        "Samples": {
          "type": "array",
          "items": {
            "type": "object",
            "properties": {
              "Position": { "$ref": "#/$defs/position" },
              "MeanSeaLevelDepth": { "type": "number", "description": "Depth in SI metres, positive downward from the EGM84 mean-sea-level geoid." },
              "GeoidUndulation": { "type": "number", "description": "EGM84 geoid undulation in SI metres, positive upward; MeanSeaLevelDepth = Wgs84EllipsoidalDepth + GeoidUndulation." }
            },
            "required": ["Position", "MeanSeaLevelDepth", "GeoidUndulation"],
            "additionalProperties": false
          }
        }
      },
      "required": ["Model", "Samples"],
      "additionalProperties": false,
      "$defs": {
        "position": {
          "type": "object",
          "properties": {
            "Latitude": { "type": "number", "description": "WGS84 geodetic latitude in SI radians." },
            "Longitude": { "type": "number", "description": "WGS84 longitude in SI radians." },
            "Wgs84EllipsoidalDepth": { "type": "number", "description": "Input depth in SI metres, positive downward from the WGS84 reference ellipsoid." }
          },
          "required": ["Latitude", "Longitude", "Wgs84EllipsoidalDepth"],
          "additionalProperties": false
        },
        "modelInfo": {
          "type": "object",
          "properties": {
            "Name": { "type": "string" },
            "ID": { "type": "string" },
            "Description": { "type": "string" },
            "DataDateTime": { "type": ["string", "null"], "format": "date-time" },
            "GridResolutionMinutes": { "type": "number" },
            "Interpolation": { "type": "string" },
            "MaximumInterpolationError": { "type": "number" },
            "RMSInterpolationError": { "type": "number" },
            "GeographicLibVersion": { "type": "string" },
            "ReferenceEllipsoid": { "type": "string", "const": "WGS84" },
            "SupportedVerticalDatums": { "type": "array", "items": { "type": "string" } },
            "SupportedConversionDirections": { "type": "array", "items": { "type": "string" } },
            "DepthPositiveDirection": { "type": "string", "const": "down" },
            "IsThreadSafe": { "type": "boolean" },
            "CoefficientSHA256": { "type": "string", "pattern": "^[0-9a-fA-F]{64}$" }
          },
          "required": ["Name", "ID", "Description", "DataDateTime", "GridResolutionMinutes", "Interpolation", "MaximumInterpolationError", "RMSInterpolationError", "GeographicLibVersion", "ReferenceEllipsoid", "SupportedVerticalDatums", "SupportedConversionDirections", "DepthPositiveDirection", "IsThreadSafe", "CoefficientSHA256"],
          "additionalProperties": false
        }
      }
    }
    """)!;
}
