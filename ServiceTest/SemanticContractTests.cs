using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Writers;
using NUnit.Framework;
using OSDC.DotnetLibraries.Drilling.SemanticCatalogue;
using OSDC.Drilling.EarthVerticalDatum.Model;
using OSDC.Drilling.EarthVerticalDatum.Service.Mcp;
using Swashbuckle.AspNetCore.Swagger;

namespace OSDC.Drilling.EarthVerticalDatum.ServiceTest;

public class SemanticContractTests
{
    [Test]
    public async Task RestGeneratedContractAndMcpAgreeWithCuratedModelBindings()
    {
        string statistics = Path.Combine(Path.GetTempPath(), "verticaldatum-semantics-" + Guid.NewGuid() + ".json");
        try
        {
            using var factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
                builder.UseSetting("EarthVerticalDatum:UsageStatisticsFile", statistics));
            using var client = factory.CreateClient();
            var tools = factory.Services.GetServices<IMcpTool>().ToDictionary(t => t.Name);
            var forward = tools["earth_vertical_datum_convert_mean_sea_level_to_wgs84"];
            var inverse = tools["earth_vertical_datum_convert_wgs84_to_mean_sea_level"];
            using var writer = new StringWriter(System.Globalization.CultureInfo.InvariantCulture);
            factory.Services.GetRequiredService<ISwaggerProvider>().GetSwagger("v1").SerializeAsV3(new OpenApiJsonWriter(writer));
            var live = JsonNode.Parse(writer.ToString())!["components"]!["schemas"]!;
            var document = JsonNode.Parse(await client.GetStringAsync("/EarthVerticalDatum/api/swagger/merged/swagger.json"))!;
            var merged = document["components"]!["schemas"]!;
            Assert.That(document["servers"]![0]!["url"]!.GetValue<string>(), Is.EqualTo("http://localhost/EarthVerticalDatum/api"));
            foreach (var (type, mcp) in new[] {
                (typeof(MeanSeaLevelToWgs84Request), forward.InputSchema),
                (typeof(MeanSeaLevelToWgs84Response), forward.OutputSchema),
                (typeof(EarthVerticalDatumPosition), forward.InputSchema["properties"]!["Positions"]!["items"]!),
                (typeof(EarthVerticalDatumPosition), forward.OutputSchema["$defs"]!["position"]!),
                (typeof(EarthVerticalDatumSample), forward.OutputSchema["properties"]!["Samples"]!["items"]!),
                (typeof(Wgs84ToMeanSeaLevelRequest), inverse.InputSchema),
                (typeof(Wgs84ToMeanSeaLevelResponse), inverse.OutputSchema),
                (typeof(Wgs84ToMeanSeaLevelPosition), inverse.InputSchema["properties"]!["Positions"]!["items"]!),
                (typeof(Wgs84ToMeanSeaLevelPosition), inverse.OutputSchema["$defs"]!["position"]!),
                (typeof(Wgs84ToMeanSeaLevelSample), inverse.OutputSchema["properties"]!["Samples"]!["items"]!),
                (typeof(EarthVerticalDatumModelInfo), forward.OutputSchema["$defs"]!["modelInfo"]!),
                (typeof(EarthVerticalDatumModelInfo), inverse.OutputSchema["$defs"]!["modelInfo"]!),
                (typeof(EarthVerticalDatumModelInfo), tools["earth_vertical_datum_get_model_info"].OutputSchema) })
            {
                foreach (var (source, schema) in new[]
                {
                    ("live REST", live[type.FullName!]!),
                    ("merged REST", merged[type.Name]!),
                    ("MCP", mcp)
                })
                {
                    JsonObject actualTypeMetadata = (JsonObject)schema[SemanticMetadata.ExtensionName]!.DeepClone();
                    if (source == "MCP" && (type == typeof(MeanSeaLevelToWgs84Request) || type == typeof(Wgs84ToMeanSeaLevelRequest)))
                    {
                        Assert.That(actualTypeMetadata["role"]!.GetValue<string>(), Is.EqualTo(Concepts.StatelessEvaluation));
                        actualTypeMetadata.Remove("role");
                    }
                    Assert.That(JsonNode.DeepEquals(actualTypeMetadata, SemanticMetadata.For(type)), Is.True,
                        $"{source}: {type.Name}; actual={schema[SemanticMetadata.ExtensionName]?.ToJsonString()}; expected={SemanticMetadata.For(type)?.ToJsonString()}");
                    Assert.That(schema[SemanticMetadata.ExtensionName]!["catalogueVersion"]!.GetValue<string>(), Is.EqualTo("0.18.0"));
                    Assert.That(schema[SemanticMetadata.ExtensionName]!["curationStatus"]!.GetValue<string>(), Is.EqualTo("Reviewed"));
                    foreach (var property in type.GetProperties())
                    {
                        var actual = schema["properties"]![property.Name]!;
                        Assert.That(JsonNode.DeepEquals(actual[SemanticMetadata.ExtensionName], SemanticMetadata.For(property)), Is.True,
                            $"{source}: {type.Name}.{property.Name}; actual={actual[SemanticMetadata.ExtensionName]?.ToJsonString()}; expected={SemanticMetadata.For(property)?.ToJsonString()}");
                        if (property.PropertyType == typeof(double))
                            Assert.That(actual[SemanticMetadata.ExtensionName]?["physicalQuantity"], Is.Not.Null, property.Name);
                    }
                }
            }
            var info = tools["earth_vertical_datum_get_model_info"].OutputSchema["properties"]!;
            Assert.That(info["GridResolutionMinutes"], Is.Null);
            Assert.That(info["AngularGridSpacing"]!["const"]!.GetValue<double>(), Is.EqualTo(Math.PI / 360).Within(1e-15));
            Assert.That(info["AngularGridSpacing"]![SemanticMetadata.ExtensionName]!["physicalQuantity"]!["name"]!.GetValue<string>(), Is.EqualTo("PlaneAngleGeodesic"));
            var undulation = SemanticMetadata.For(typeof(EarthVerticalDatumSample).GetProperty("GeoidUndulation")!)!;
            Assert.That(undulation["physicalQuantity"]!["name"]!.GetValue<string>(), Is.EqualTo("LengthStandard"));
            var api = new ModelShared.Client("http://localhost/EarthVerticalDatum/api/", client);
            Assert.That((await api.GetEarthVerticalDatumModelInfoAsync()).AngularGridSpacing, Is.EqualTo(Math.PI / 360).Within(1e-15));
        }
        finally
        {
            File.Delete(statistics);
        }
    }
}
