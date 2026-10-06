using System.Reflection;
using System.Text.Json.Nodes;
using OSDC.DotnetLibraries.Drilling.SemanticCatalogue;

namespace OSDC.Drilling.EarthVerticalDatum.Service.Mcp;

internal static class McpSemanticSchemas
{
    public static JsonNode Annotate(JsonNode schema, Type modelType)
    {
        if (schema is JsonObject root) Walk(root, modelType);
        return schema;
    }

    private static void Walk(JsonObject schema, Type type)
    {
        type = Nullable.GetUnderlyingType(type) ?? type;
        if (type.IsGenericType && typeof(System.Collections.IEnumerable).IsAssignableFrom(type))
        {
            if (schema["items"] is JsonObject items) Walk(items, type.GetGenericArguments().Last());
            return;
        }
        if (SemanticMetadata.For(type) is { } typeMetadata)
            schema[SemanticMetadata.ExtensionName] = typeMetadata;
        if (schema["properties"] is not JsonObject properties) return;
        foreach (PropertyInfo property in type.GetProperties())
        {
            if (properties[property.Name] is not JsonObject target) continue;
            if (SemanticMetadata.For(property) is { } metadata)
                target[SemanticMetadata.ExtensionName] = metadata;
            Walk(target, property.PropertyType);
        }
    }
}
