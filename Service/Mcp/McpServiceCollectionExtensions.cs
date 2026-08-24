using ModelContextProtocol.Server;

namespace OSDC.Drilling.EarthVerticalDatum.Service.Mcp;

internal static class McpServiceCollectionExtensions
{
    public static IServiceCollection AddEarthVerticalDatumMcpTool<T>(this IServiceCollection services)
        where T : class, IMcpTool
    {
        services.AddSingleton<T>();
        services.AddSingleton<IMcpTool>(provider => provider.GetRequiredService<T>());
        services.AddSingleton<McpServerTool>(provider => new McpServerToolAdapter(
            provider.GetRequiredService<T>(), provider.GetRequiredService<ILoggerFactory>()));
        return services;
    }
}
