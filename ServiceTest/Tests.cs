using System.Net;
using System.Text;
using System.Text.Json;
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

    [SetUp]
    public void Setup()
    {
        factory_ = new WebApplicationFactory<Program>();
        httpClient_ = factory_.CreateClient();
        generatedClient_ = new Client("http://localhost/EarthVerticalDatum/api/", httpClient_);
    }

    [TearDown]
    public void TearDown()
    {
        httpClient_.Dispose();
        factory_.Dispose();
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
        });
    }

    [Test]
    public void UsageStatisticsAreNotRegisteredAsMcpTools()
    {
        string[] names = factory_.Services.GetServices<IMcpTool>().Select(tool => tool.Name).Order().ToArray();
        Assert.That(names, Is.EqualTo(new[]
        {
            "earth_vertical_datum_convert_mean_sea_level_to_wgs84",
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
            Assert.That(tools.GetArrayLength(), Is.EqualTo(3));
            Assert.That(tools.EnumerateArray().All(tool => tool.TryGetProperty("inputSchema", out _)), Is.True);
            Assert.That(tools.EnumerateArray().All(tool => tool.TryGetProperty("outputSchema", out _)), Is.True);
            Assert.That(content, Does.Not.Contain("usage_statistics").IgnoreCase);
        });

        JsonElement convert = tools.EnumerateArray().Single(tool =>
            tool.GetProperty("name").GetString() == "earth_vertical_datum_convert_mean_sea_level_to_wgs84");
        Assert.Multiple(() =>
        {
            Assert.That(convert.GetProperty("description").GetString(), Does.Contain("positive downward"));
            Assert.That(convert.GetProperty("description").GetString(), Does.Contain("no GUID"));
            Assert.That(convert.GetProperty("outputSchema").GetProperty("properties")
                .TryGetProperty("Samples", out _), Is.True);
        });
    }

    [Test]
    public async Task McpConversionToolReturnsStructuredResult()
    {
        const string payload = """
            {"jsonrpc":"2.0","id":2,"method":"tools/call","params":{"name":"earth_vertical_datum_convert_mean_sea_level_to_wgs84","arguments":{"Positions":[{"Latitude":0.5,"Longitude":1.0,"MeanSeaLevelDepth":1000.0}]}}}
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
            Assert.That(structured.GetProperty("Samples")[0].GetProperty("Wgs84EllipsoidalDepth").GetDouble(),
                Is.Not.EqualTo(1000.0));
        });
    }

    [TestCase("/EarthVerticalDatum/api/health/live")]
    [TestCase("/EarthVerticalDatum/api/health/ready")]
    [TestCase("/EarthVerticalDatum/api/metrics")]
    [TestCase("/EarthVerticalDatum/api/swagger/merged/swagger.json")]
    public async Task OperationalEndpointsAreAvailable(string path) =>
        Assert.That((await httpClient_.GetAsync(path)).StatusCode, Is.EqualTo(HttpStatusCode.OK));
}
