using System.Text.Json;
using System.Text.Json.Nodes;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using OSDC.Drilling.EarthVerticalDatum.Model;

namespace OSDC.Drilling.EarthVerticalDatum.Service.Mcp;

internal sealed class McpServerToolAdapter : McpServerTool
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = null,
        DictionaryKeyPolicy = null,
        PropertyNameCaseInsensitive = true
    };
    private readonly IMcpTool tool_;
    private readonly ILogger logger_;
    private readonly Tool protocolTool_;

    public McpServerToolAdapter(IMcpTool tool, ILoggerFactory loggerFactory)
    {
        tool_ = tool;
        logger_ = loggerFactory.CreateLogger(tool.GetType());
        protocolTool_ = new Tool
        {
            Name = tool.Name,
            Description = tool.Description,
            InputSchema = JsonSerializer.SerializeToElement(tool.InputSchema, JsonOptions),
            OutputSchema = JsonSerializer.SerializeToElement(tool.OutputSchema, JsonOptions),
            // All registered tools inspect or calculate from the bundled model. Usage
            // counters do not change the domain result or create calculation resources.
            Annotations = new ToolAnnotations
            {
                ReadOnlyHint = true,
                DestructiveHint = false,
                IdempotentHint = true,
                OpenWorldHint = false
            }
        };
    }

    public override Tool ProtocolTool => protocolTool_;
    public override IReadOnlyList<object> Metadata { get; } = Array.Empty<object>();

    public override async ValueTask<CallToolResult> InvokeAsync(
        RequestContext<CallToolRequestParams> request, CancellationToken cancellationToken = default)
    {
        var arguments = new JsonObject();
        if (request.Params?.Arguments is { } suppliedArguments)
        {
            foreach ((string name, JsonElement value) in suppliedArguments)
                arguments[name] = JsonNode.Parse(value.GetRawText());
        }

        try
        {
            JsonNode? result = await tool_.InvokeAsync(arguments, cancellationToken).ConfigureAwait(false);
            if (result is null)
                return new CallToolResult();

            string serializedResult = result.ToJsonString(JsonOptions);
            return new CallToolResult
            {
                StructuredContent = JsonSerializer.Deserialize<JsonElement>(serializedResult, JsonOptions),
                // MCP clients that negotiate a protocol version before structured tool output
                // was introduced still consume the JSON result from a text content block.
                Content = { new TextContentBlock { Text = serializedResult } }
            };
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (EarthVerticalDatumValidationException exception)
        {
            JsonNode problem = JsonSerializer.SerializeToNode(new EarthVerticalDatumValidationProblem
            {
                Message = exception.Message,
                Errors = exception.Errors.ToList()
            }, JsonOptions)!;
            return new CallToolResult
            {
                IsError = true,
                StructuredContent = JsonSerializer.SerializeToElement(problem, JsonOptions),
                Content = { new TextContentBlock { Text = problem.ToJsonString(JsonOptions) } }
            };
        }
        catch (Exception exception)
        {
            logger_.LogWarning(exception, "MCP tool {ToolName} failed.", tool_.Name);
            return new CallToolResult
            {
                IsError = true,
                Content = { new TextContentBlock { Text = exception.Message } }
            };
        }
    }
}
