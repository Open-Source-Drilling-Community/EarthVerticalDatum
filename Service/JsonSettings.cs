using System.Text.Json;
using System.Text.Json.Serialization;

namespace OSDC.Drilling.EarthVerticalDatum.Service;

public static class JsonSettings
{
    public static JsonSerializerOptions Options { get; } = CreateOptions();

    public static void ApplyTo(JsonSerializerOptions options)
    {
        options.PropertyNamingPolicy = null;
        options.DictionaryKeyPolicy = null;
        options.PropertyNameCaseInsensitive = true;
        options.Converters.Add(new JsonStringEnumConverter());
    }

    private static JsonSerializerOptions CreateOptions()
    {
        JsonSerializerOptions options = new();
        ApplyTo(options);
        return options;
    }
}
