# Service

`Service` is the stateless ASP.NET Core host for Earth Vertical Datum REST and MCP APIs. It loads EGM84-30 at startup and has no persistence dependency.

## Endpoints

- `GET /EarthVerticalDatum/api/EarthVerticalDatum`: discovery entry point returning model information.
- `POST /EarthVerticalDatum/api/EarthVerticalDatum/ConvertMeanSeaLevelToWgs84`: synchronous atomic batch conversion.
- `POST /EarthVerticalDatum/api/EarthVerticalDatum/ConvertWgs84ToMeanSeaLevel`: synchronous atomic inverse batch conversion.
- `GET /EarthVerticalDatum/api/EarthVerticalDatum/ModelInfo`: model identity and provenance.
- `GET /EarthVerticalDatum/api/EarthVerticalDatumUsageStatistics`: process-replica counters; REST only.
- `/EarthVerticalDatum/api/mcp`: MCP Streamable HTTP.
- `/EarthVerticalDatum/api/health`, `/metrics`, `/swagger`, and `/json-schema/EarthVerticalDatumMergedModel.json`.

Configuration section `EarthVerticalDatum` supports `MaximumPositionsPerRequest` (default 10000) and optional `ModelDirectory`. The default directory is `VerticalDatumModelFiles` beside the application.

MCP publishes only `ping`, `earth_vertical_datum_get_model_info`, `earth_vertical_datum_convert_mean_sea_level_to_wgs84`, and `earth_vertical_datum_convert_wgs84_to_mean_sea_level`. Descriptions and schemas document radians, metre/positive-down depths, positive-up geoid undulation, stateless execution, ordering, and atomic errors. Usage statistics are not an MCP tool.

Run `dotnet run --project Service/Service.csproj`. The image is `digiwells/osdcdrillingearthverticaldatumservice`; the chart is under `charts/osdcdrillingearthverticaldatumservice` and creates no persistence or PodDisruptionBudget resource.

Author: Eric Cayeux

Company: NORCE Research
