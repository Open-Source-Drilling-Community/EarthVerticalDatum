using System.Text.Json.Nodes;

namespace OSDC.Drilling.EarthVerticalDatum.Service.Mcp.Tools;

public sealed class PingMcpTool : IMcpTool
{
    public string Name => "ping";
    public string Description => "Checks whether the stateless OSDC Earth Vertical Datum MCP server is reachable. It performs no vertical-datum conversion, reads no model coefficients, persists nothing, and does not expose usage statistics. A successful result is {\"Status\":\"ok\",\"Service\":\"OSDC Earth Vertical Datum\"}.";
    public JsonNode InputSchema { get; } = JsonNode.Parse("""{"type":"object","properties":{},"additionalProperties":false}""")!;
    public JsonNode OutputSchema { get; } = JsonNode.Parse("""
    {
      "type": "object",
      "description": "Successful Earth Vertical Datum service reachability result.",
      "properties": {
        "Status": { "type": "string", "const": "ok", "description": "The fixed success status." },
        "Service": { "type": "string", "const": "OSDC Earth Vertical Datum", "description": "The responding service name." }
      },
      "required": ["Status", "Service"],
      "additionalProperties": false
    }
    """)!;

    public Task<JsonNode?> InvokeAsync(JsonObject? arguments, CancellationToken cancellationToken) =>
        Task.FromResult<JsonNode?>(new JsonObject { ["Status"] = "ok", ["Service"] = "OSDC Earth Vertical Datum" });
}
