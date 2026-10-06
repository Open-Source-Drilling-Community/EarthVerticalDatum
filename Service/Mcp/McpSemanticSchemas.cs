using System.Reflection;
using System.Text.Json.Nodes;
using OSDC.DotnetLibraries.Drilling.SemanticCatalogue;

namespace OSDC.Drilling.EarthVerticalDatum.Service.Mcp;

internal static class McpSemanticSchemas
{
    public static JsonNode Annotate(JsonNode schema, Type modelType)
    {
        if (schema is JsonObject root) Walk(root, root, modelType);
        return schema;
    }

    private static void Walk(JsonObject root, JsonObject schema, Type type)
    {
        type = Nullable.GetUnderlyingType(type) ?? type;
        if (type.IsGenericType && typeof(System.Collections.IEnumerable).IsAssignableFrom(type))
        {
            if (schema["items"] is JsonObject items) Walk(root, Resolve(root, items), type.GetGenericArguments().Last());
            return;
        }
        schema = Resolve(root, schema);
        if (SemanticMetadata.For(type) is { } typeMetadata)
            schema[SemanticMetadata.ExtensionName] = typeMetadata;
        if (schema["properties"] is not JsonObject properties) return;
        foreach (PropertyInfo property in type.GetProperties())
        {
            if (properties[property.Name] is not JsonObject target) continue;
            Walk(root, target, property.PropertyType);
            if (SemanticMetadata.For(property) is { } metadata)
                target[SemanticMetadata.ExtensionName] = metadata;
        }
    }

    private static JsonObject Resolve(JsonObject root, JsonObject schema)
    {
        if (schema["$ref"]?.GetValue<string>() is not { } reference ||
            !reference.StartsWith("#/$defs/", StringComparison.Ordinal))
            return schema;
        string name = reference["#/$defs/".Length..];
        return root["$defs"]?[name] as JsonObject ?? schema;
    }
}
