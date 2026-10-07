using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using OSDC.Drilling.EarthVerticalDatum.ModelShared;
using OSDC.Drilling.EarthVerticalDatum.Service.Mcp;

namespace OSDC.Drilling.EarthVerticalDatum.ServiceTest;

public class Tests
{
    private WebApplicationFactory<Program> factory_ = null!;
    private HttpClient httpClient_ = null!;
    private Client generatedClient_ = null!;
    private string statisticsFile_ = null!;

    [SetUp]
    public void Setup()
    {
        statisticsFile_ = Path.Combine(Path.GetTempPath(), "earthverticaldatum-tests", Guid.NewGuid().ToString(), "statistics.json");
        factory_ = CreateFactory(statisticsFile_);
        httpClient_ = factory_.CreateClient();
        generatedClient_ = new Client("http://localhost/EarthVerticalDatum/api/", httpClient_);
    }

    [TearDown]
    public void TearDown()
    {
        httpClient_.Dispose();
        factory_.Dispose();
        string? directory = Path.GetDirectoryName(statisticsFile_);
        if (directory is not null && Directory.Exists(directory) &&
            Path.GetFullPath(directory).StartsWith(Path.GetFullPath(Path.GetTempPath()), StringComparison.OrdinalIgnoreCase))
        {
            Directory.Delete(directory, true);
        }
    }

    [Test]
    public async Task GeneratedClientConvertsMeanSeaLevelDepth()
    {
        var request = PseudoConstructors.ConstructMeanSeaLevelToWgs84Request();
        request.Positions.First().Latitude = 0.5;
        request.Positions.First().Longitude = 1.0;
        request.Positions.First().MeanSeaLevelDepth = 1000;

        MeanSeaLevelToWgs84Response response = await generatedClient_.ConvertMeanSeaLevelToWgs84Async(request);
        Assert.Multiple(() =>
        {
            Assert.That(response.Samples, Has.Count.EqualTo(1));
            Assert.That(response.Samples.First().Wgs84EllipsoidalDepth, Is.Not.EqualTo(1000));
            Assert.That(response.Model.ID, Is.EqualTo("EGM84-30"));
        });
    }

    [Test]
    public async Task UsageStatisticsSurviveServiceRestart()
    {
        string statisticsFile = Path.Combine(Path.GetTempPath(), "earthverticaldatum-restart-tests",
            Guid.NewGuid().ToString(), "statistics.json");
        try
        {
            DateTimeOffset startedAt;
            using (var firstFactory = CreateFactory(statisticsFile))
            using (HttpClient firstHttpClient = firstFactory.CreateClient())
            {
                var firstClient = new Client("http://localhost/EarthVerticalDatum/api/", firstHttpClient);
                await firstClient.GetEarthVerticalDatumEntryAsync();
                await firstClient.ConvertMeanSeaLevelToWgs84Async(
                    PseudoConstructors.ConstructMeanSeaLevelToWgs84Request());
                UsageStatisticsEarthVerticalDatum beforeRestart =
                    await firstClient.GetEarthVerticalDatumUsageStatisticsAsync();
                startedAt = beforeRestart.StartedAt;
                Assert.Multiple(() =>
                {
                    Assert.That(beforeRestart.ModelInfoRequests, Is.GreaterThanOrEqualTo(1));
                    Assert.That(beforeRestart.RestConversions, Is.EqualTo(1));
                    Assert.That(beforeRestart.PositionsConverted, Is.EqualTo(1));
                });
            }

            Assert.That(File.Exists(statisticsFile), Is.True);

            using var secondFactory = CreateFactory(statisticsFile);
            using HttpClient secondHttpClient = secondFactory.CreateClient();
            var secondClient = new Client("http://localhost/EarthVerticalDatum/api/", secondHttpClient);
            UsageStatisticsEarthVerticalDatum restored =
                await secondClient.GetEarthVerticalDatumUsageStatisticsAsync();
            Assert.Multiple(() =>
            {
                Assert.That(restored.ModelInfoRequests, Is.GreaterThanOrEqualTo(1));
                Assert.That(restored.RestConversions, Is.EqualTo(1));
                Assert.That(restored.PositionsConverted, Is.EqualTo(1));
                Assert.That(restored.StartedAt, Is.EqualTo(startedAt));
                Assert.That(restored.Scope, Is.EqualTo("persistent-service"));
            });
        }
        finally
        {
            string? directory = Path.GetDirectoryName(statisticsFile);
            if (directory is not null && Directory.Exists(directory)) Directory.Delete(directory, true);
        }
    }

    [Test]
    public async Task GeneratedClientConvertsWgs84EllipsoidalDepth()
    {
        var request = PseudoConstructors.ConstructWgs84ToMeanSeaLevelRequest();
        request.Positions.First().Latitude = 0.5;
        request.Positions.First().Longitude = 1.0;
        request.Positions.First().Wgs84EllipsoidalDepth = 1000;

        Wgs84ToMeanSeaLevelResponse response = await generatedClient_.ConvertWgs84ToMeanSeaLevelAsync(request);
        Assert.Multiple(() =>
        {
            Assert.That(response.Samples, Has.Count.EqualTo(1));
            Assert.That(response.Samples.First().MeanSeaLevelDepth, Is.Not.EqualTo(1000));
            Assert.That(response.Model.ID, Is.EqualTo("EGM84-30"));
        });
    }

    [Test]
    public void InvalidRequestReturnsUnprocessableEntityThroughGeneratedClient()
    {
        var request = PseudoConstructors.ConstructMeanSeaLevelToWgs84Request();
        request.Positions.First().Latitude = Math.PI;
        ApiException exception = Assert.CatchAsync<ApiException>(async () =>
            await generatedClient_.ConvertMeanSeaLevelToWgs84Async(request))!;
        Assert.That(exception.StatusCode, Is.EqualTo((int)HttpStatusCode.UnprocessableEntity));
    }

    [TestCase("/EarthVerticalDatum/api/EarthVerticalDatum")]
    [TestCase("/earthverticaldatum/api/earthverticaldatum")]
    public async Task ServiceEntryEndpointReturnsModelInformation(string path)
    {
        HttpResponseMessage response = await httpClient_.GetAsync(path);
        using JsonDocument document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Multiple(() =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(document.RootElement.GetProperty("ID").GetString(), Is.EqualTo("EGM84-30"));
            Assert.That(document.RootElement.GetProperty("DepthPositiveDirection").GetString(), Is.EqualTo("down"));
            Assert.That(document.RootElement.GetProperty("AngularGridSpacing").GetDouble(), Is.EqualTo(Math.PI / 360.0));
            Assert.That(document.RootElement.TryGetProperty("GridResolutionMinutes", out _), Is.False);
        });
    }

    [Test]
    public void UsageStatisticsAreNotRegisteredAsMcpTools()
    {
        string[] names = factory_.Services.GetServices<IMcpTool>().Select(tool => tool.Name).Order().ToArray();
        Assert.That(names, Is.EqualTo(new[]
        {
            "earth_vertical_datum_convert_mean_sea_level_to_wgs84",
            "earth_vertical_datum_convert_wgs84_to_mean_sea_level",
            "earth_vertical_datum_get_model_info",
            "ping"
        }));
    }

    [Test]
    public async Task McpToolListPublishesCompleteSchemasWithoutUsageStatistics()
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, "/EarthVerticalDatum/api/mcp");
        request.Headers.Accept.ParseAdd("application/json");
        request.Headers.Accept.ParseAdd("text/event-stream");
        request.Content = new StringContent(
            """{"jsonrpc":"2.0","id":1,"method":"tools/list","params":{}}""", Encoding.UTF8, "application/json");
        HttpResponseMessage response = await httpClient_.SendAsync(request);
        string content = await response.Content.ReadAsStringAsync();

        string dataLine = content.Split('\n', StringSplitOptions.RemoveEmptyEntries)
            .Single(line => line.StartsWith("data:", StringComparison.Ordinal));
        using JsonDocument document = JsonDocument.Parse(dataLine["data:".Length..].Trim());
        JsonElement tools = document.RootElement.GetProperty("result").GetProperty("tools");

        Assert.Multiple(() =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(tools.GetArrayLength(), Is.EqualTo(4));
            Assert.That(tools.EnumerateArray().All(tool => tool.TryGetProperty("inputSchema", out _)), Is.True);
            Assert.That(tools.EnumerateArray().All(tool => tool.TryGetProperty("outputSchema", out _)), Is.True);
            foreach (JsonElement tool in tools.EnumerateArray())
            {
                JsonElement annotations = tool.GetProperty("annotations");
                Assert.That(annotations.GetProperty("readOnlyHint").GetBoolean(), Is.True);
                Assert.That(annotations.GetProperty("destructiveHint").GetBoolean(), Is.False);
                Assert.That(annotations.GetProperty("idempotentHint").GetBoolean(), Is.True);
                Assert.That(annotations.GetProperty("openWorldHint").GetBoolean(), Is.False);
            }
            Assert.That(content, Does.Not.Contain("usage_statistics").IgnoreCase);
        });

        JsonElement convert = tools.EnumerateArray().Single(tool =>
            tool.GetProperty("name").GetString() == "earth_vertical_datum_convert_mean_sea_level_to_wgs84");
        JsonElement inverse = tools.EnumerateArray().Single(tool =>
            tool.GetProperty("name").GetString() == "earth_vertical_datum_convert_wgs84_to_mean_sea_level");
        Assert.Multiple(() =>
        {
            Assert.That(convert.GetProperty("description").GetString(), Does.Contain("positive downward"));
            Assert.That(convert.GetProperty("description").GetString(), Does.Contain("no GUID"));
            Assert.That(convert.GetProperty("outputSchema").GetProperty("properties")
                .TryGetProperty("Samples", out _), Is.True);
            Assert.That(convert.GetProperty("outputSchema").GetProperty("$defs")
                .GetProperty("modelInfo").GetProperty("properties")
                .TryGetProperty("AngularGridSpacing", out _), Is.True);
            Assert.That(convert.GetProperty("outputSchema").GetProperty("$defs")
                .GetProperty("modelInfo").GetProperty("properties")
                .TryGetProperty("GridResolutionMinutes", out _), Is.False);
            Assert.That(inverse.GetProperty("description").GetString(), Does.Contain("positive downward"));
            Assert.That(inverse.GetProperty("description").GetString(), Does.Contain("MeanSeaLevelDepth = Wgs84EllipsoidalDepth + GeoidUndulation"));
            Assert.That(inverse.GetProperty("inputSchema").GetProperty("properties")
                .GetProperty("Positions").GetProperty("items").GetProperty("properties")
                .TryGetProperty("Wgs84EllipsoidalDepth", out _), Is.True);
        });
    }

    [Test]
    public async Task McpConversionToolReturnsStructuredAndBackwardCompatibleTextResult()
    {
        const string payload = """
            {"jsonrpc":"2.0","id":2,"method":"tools/call","params":{"name":"earth_vertical_datum_convert_mean_sea_level_to_wgs84","arguments":{"Positions":[{"Latitude":0.5,"Longitude":1.0,"MeanSeaLevelDepth":1000.0}]}}}
            """;
        using var request = new HttpRequestMessage(HttpMethod.Post, "/EarthVerticalDatum/api/mcp");
        request.Headers.TryAddWithoutValidation("MCP-Protocol-Version", "2025-03-26");
        request.Headers.Accept.ParseAdd("application/json");
        request.Headers.Accept.ParseAdd("text/event-stream");
        request.Content = new StringContent(payload, Encoding.UTF8, "application/json");

        HttpResponseMessage response = await httpClient_.SendAsync(request);
        string content = await response.Content.ReadAsStringAsync();
        string dataLine = content.Split('\n', StringSplitOptions.RemoveEmptyEntries)
            .Single(line => line.StartsWith("data:", StringComparison.Ordinal));
        using JsonDocument document = JsonDocument.Parse(dataLine["data:".Length..].Trim());
        JsonElement result = document.RootElement.GetProperty("result");
        JsonElement structured = result.GetProperty("structuredContent");
        JsonElement textContent = result.GetProperty("content")[0];
        using JsonDocument textDocument = JsonDocument.Parse(textContent.GetProperty("text").GetString()!);
        string dataDateTime = structured.GetProperty("Model").GetProperty("DataDateTime").GetString()!;

        Assert.Multiple(() =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(result.TryGetProperty("isError", out JsonElement isError) && isError.GetBoolean(), Is.False);
            Assert.That(structured.GetProperty("Model").GetProperty("ID").GetString(), Is.EqualTo("EGM84-30"));
            Assert.That(structured.GetProperty("Samples")[0].GetProperty("Wgs84EllipsoidalDepth").GetDouble(),
                Is.Not.EqualTo(1000.0));
            Assert.That(textContent.GetProperty("type").GetString(), Is.EqualTo("text"));
            Assert.That(JsonElement.DeepEquals(textDocument.RootElement, structured), Is.True);
            Assert.That(dataDateTime, Does.EndWith("Z"));
            Assert.That(DateTimeOffset.TryParse(dataDateTime, out _), Is.True);
        });
    }

    [Test]
    public async Task McpConversionToolAcceptsReportedMultiPositionPayload()
    {
        JsonNode arguments = JsonNode.Parse(await File.ReadAllTextAsync(
            Path.Combine(AppContext.BaseDirectory, "TestData", "vertical_datum_request.json")))!;
        string payload = new JsonObject
        {
            ["jsonrpc"] = "2.0",
            ["id"] = 4,
            ["method"] = "tools/call",
            ["params"] = new JsonObject
            {
                ["name"] = "earth_vertical_datum_convert_mean_sea_level_to_wgs84",
                ["arguments"] = arguments
            }
        }.ToJsonString();
        using var request = new HttpRequestMessage(HttpMethod.Post, "/EarthVerticalDatum/api/mcp");
        request.Headers.TryAddWithoutValidation("MCP-Protocol-Version", "2025-03-26");
        request.Headers.Accept.ParseAdd("application/json");
        request.Headers.Accept.ParseAdd("text/event-stream");
        request.Content = new StringContent(payload, Encoding.UTF8, "application/json");

        HttpResponseMessage response = await httpClient_.SendAsync(request);
        string content = await response.Content.ReadAsStringAsync();
        string dataLine = content.Split('\n', StringSplitOptions.RemoveEmptyEntries)
            .Single(line => line.StartsWith("data:", StringComparison.Ordinal));
        using JsonDocument document = JsonDocument.Parse(dataLine["data:".Length..].Trim());
        JsonElement result = document.RootElement.GetProperty("result");
        JsonElement structured = result.GetProperty("structuredContent");
        JsonElement textContent = result.GetProperty("content")[0];

        Assert.Multiple(() =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(result.TryGetProperty("isError", out JsonElement isError) && isError.GetBoolean(), Is.False);
            Assert.That(structured.GetProperty("Samples").GetArrayLength(), Is.EqualTo(55));
            Assert.That(textContent.GetProperty("type").GetString(), Is.EqualTo("text"));
            Assert.That(textContent.GetProperty("text").GetString(), Does.Contain("Wgs84EllipsoidalDepth"));
        });
    }

    [Test]
    public async Task McpInverseConversionToolReturnsStructuredResult()
    {
        const string payload = """
            {"jsonrpc":"2.0","id":3,"method":"tools/call","params":{"name":"earth_vertical_datum_convert_wgs84_to_mean_sea_level","arguments":{"Positions":[{"Latitude":0.5,"Longitude":1.0,"Wgs84EllipsoidalDepth":1000.0}]}}}
            """;
        using var request = new HttpRequestMessage(HttpMethod.Post, "/EarthVerticalDatum/api/mcp");
        request.Headers.Accept.ParseAdd("application/json");
        request.Headers.Accept.ParseAdd("text/event-stream");
        request.Content = new StringContent(payload, Encoding.UTF8, "application/json");

        HttpResponseMessage response = await httpClient_.SendAsync(request);
        string content = await response.Content.ReadAsStringAsync();
        string dataLine = content.Split('\n', StringSplitOptions.RemoveEmptyEntries)
            .Single(line => line.StartsWith("data:", StringComparison.Ordinal));
        using JsonDocument document = JsonDocument.Parse(dataLine["data:".Length..].Trim());
        JsonElement result = document.RootElement.GetProperty("result");
        JsonElement structured = result.GetProperty("structuredContent");

        Assert.Multiple(() =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(result.TryGetProperty("isError", out JsonElement isError) && isError.GetBoolean(), Is.False);
            Assert.That(structured.GetProperty("Model").GetProperty("ID").GetString(), Is.EqualTo("EGM84-30"));
            Assert.That(structured.GetProperty("Samples")[0].GetProperty("MeanSeaLevelDepth").GetDouble(),
                Is.Not.EqualTo(1000.0));
        });
    }

    [TestCase("/EarthVerticalDatum/api/health/live")]
    [TestCase("/EarthVerticalDatum/api/health/ready")]
    [TestCase("/EarthVerticalDatum/api/metrics")]
    [TestCase("/EarthVerticalDatum/api/swagger/merged/swagger.json")]
    public async Task OperationalEndpointsAreAvailable(string path) =>
        Assert.That((await httpClient_.GetAsync(path)).StatusCode, Is.EqualTo(HttpStatusCode.OK));

    private static WebApplicationFactory<Program> CreateFactory(string statisticsFile) =>
        new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.UseSetting("EarthVerticalDatum:UsageStatisticsFile", statisticsFile);
            builder.UseSetting("EarthVerticalDatum:UsageStatisticsSaveIntervalSeconds", "3600");
        });
}
