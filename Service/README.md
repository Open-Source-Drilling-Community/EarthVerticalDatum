# Service

`Service` is the ASP.NET Core host for stateless Earth Vertical Datum REST and MCP calculations. It loads EGM84-30 at startup; only aggregate usage counters are persisted.

## Endpoints

- `GET /EarthVerticalDatum/api/EarthVerticalDatum`: discovery entry point returning model information.
- `POST /EarthVerticalDatum/api/EarthVerticalDatum/ConvertMeanSeaLevelToWgs84`: synchronous atomic batch conversion.
- `POST /EarthVerticalDatum/api/EarthVerticalDatum/ConvertWgs84ToMeanSeaLevel`: synchronous atomic inverse batch conversion.
- `GET /EarthVerticalDatum/api/EarthVerticalDatum/ModelInfo`: model identity and provenance.
- `GET /EarthVerticalDatum/api/EarthVerticalDatumUsageStatistics`: cumulative REST and MCP counters retained across restarts; exposed through REST only.
- `/EarthVerticalDatum/api/mcp`: MCP Streamable HTTP.
- `GET /EarthVerticalDatum/api/health/live` and `GET /EarthVerticalDatum/api/health/ready`: liveness and model-readiness probes.
- `GET /EarthVerticalDatum/api/metrics`: Prometheus counters.
- `/EarthVerticalDatum/api/swagger` and `/EarthVerticalDatum/api/swagger/merged/swagger.json`: Swagger UI and the merged OpenAPI document.

Configuration section `EarthVerticalDatum` supports `MaximumPositionsPerRequest` (default 10000), optional `ModelDirectory`, `UsageStatisticsFile` (default `home/EarthVerticalDatum.UsageStatistics.json`), and `UsageStatisticsSaveIntervalSeconds` (default 30). The default model directory is `VerticalDatumModelFiles` beside the application. Changed counters are atomically saved on the interval and during graceful shutdown, then restored on startup. An abrupt failure can lose at most the changes since the last periodic save.

MCP publishes only `ping`, `earth_vertical_datum_get_model_info`, `earth_vertical_datum_convert_mean_sea_level_to_wgs84`, and `earth_vertical_datum_convert_wgs84_to_mean_sea_level`. Descriptions and schemas document radians, metre/positive-down depths, positive-up geoid undulation, stateless execution, ordering, and atomic errors. Usage statistics are not an MCP tool.

Run `dotnet run --project Service/Service.csproj`. The image is `digiwells/osdcdrillingearthverticaldatumservice` and declares `/home` as its data volume. The chart under `charts/osdcdrillingearthverticaldatumservice` creates a 1 GiB `ReadWriteOnce` claim by default and no PodDisruptionBudget. Because the snapshot has one writer, persisted deployments must use one service replica.

Author: Eric Cayeux

Company: NORCE Research
