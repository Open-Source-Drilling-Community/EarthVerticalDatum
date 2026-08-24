# OSDC Earth Vertical Datum

OSDC Earth Vertical Datum is a stateless .NET 8 microservice that converts depths in both directions between the EGM84 mean-sea-level geoid and the WGS84 reference ellipsoid. It provides synchronous REST and MCP interfaces, a generated shared client, reusable unit-aware Blazor pages, a WebApp, Docker images, and Helm charts.

The service intentionally has no database, stored datasets, calculation orders, or GUID-based retrieval workflow. A conversion request returns its result directly.

## Solution structure

- `Model`: public contracts, validation, usage counters, and the GeographicLib EGM84-30 evaluator.
- `Service`: REST, MCP, health, metrics, usage statistics, OpenAPI, and JSON Schema endpoints.
- `ModelSharedOut`: generated C# client and committed OpenAPI/JSON Schema artifacts.
- `WebPages`: reusable Razor class library with OSDC unit-system controls.
- `WebApp`: server-side Blazor host for `WebPages`.
- `ModelTest` and `ServiceTest`: model, concurrency, API, MCP, and operational tests.
- `VerticalDatumModelFiles`: the GeographicLib `egm84-30` grid and metadata.

## Public convention

- `Latitude`: WGS84 geodetic latitude in SI radians, from `-pi/2` to `pi/2`.
- `Longitude`: WGS84 longitude in SI radians, from `-pi` to `pi`.
- `MeanSeaLevelDepth`: metres, positive downward from the EGM84 mean-sea-level geoid.
- `Wgs84EllipsoidalDepth`: metres, positive downward from the WGS84 reference ellipsoid.
- `GeoidUndulation`: metres, positive upward. Therefore `Wgs84EllipsoidalDepth = MeanSeaLevelDepth - GeoidUndulation` and, inversely, `MeanSeaLevelDepth = Wgs84EllipsoidalDepth + GeoidUndulation`.

GeographicLib expects angles in degrees and heights positive upward. Those conversions occur privately at the library boundary; REST, MCP, generated contracts, and the model API retain the OSDC SI and positive-down conventions.

The loaded `egm84-30` grid uses cubic interpolation. Thread-safe mode loads this small grid into memory once at startup, after which requests can execute concurrently.

## Run locally

Prerequisites are the .NET 8 SDK and the repository files.

```powershell
dotnet restore EarthVerticalDatum.sln
dotnet run --project Service/Service.csproj
```

The local service endpoints are:

- REST discovery/model information: `http://localhost:58948/EarthVerticalDatum/api/EarthVerticalDatum`
- MSL → WGS84 conversion: `POST http://localhost:58948/EarthVerticalDatum/api/EarthVerticalDatum/ConvertMeanSeaLevelToWgs84`
- WGS84 → MSL conversion: `POST http://localhost:58948/EarthVerticalDatum/api/EarthVerticalDatum/ConvertWgs84ToMeanSeaLevel`
- MCP Streamable HTTP: `http://localhost:58948/EarthVerticalDatum/api/mcp`
- health: `http://localhost:58948/EarthVerticalDatum/api/health`
- Prometheus metrics: `http://localhost:58948/EarthVerticalDatum/api/metrics`
- Swagger UI: `http://localhost:58948/EarthVerticalDatum/api/swagger`

Example conversion:

```powershell
$body = '{"Positions":[{"Latitude":1.0471975511965976,"Longitude":0.17453292519943295,"MeanSeaLevelDepth":1000.0}]}'
Invoke-RestMethod -Method Post -ContentType application/json -Body $body `
  -Uri http://localhost:58948/EarthVerticalDatum/api/EarthVerticalDatum/ConvertMeanSeaLevelToWgs84
```

Run the WebApp separately with `dotnet run --project WebApp/WebApp.csproj`; browse to `http://localhost:58950/EarthVerticalDatum/webapp/Home`. Its development configuration calls the local service on port `58948`.

## MCP tools

The MCP server publishes exactly four underscore-named tools:

- `ping`: connectivity only.
- `earth_vertical_datum_get_model_info`: EGM84-30 identity, interpolation accuracy, runtime version, conventions, thread-safety, and grid SHA-256.
- `earth_vertical_datum_convert_mean_sea_level_to_wgs84`: synchronous stateless batch conversion.
- `earth_vertical_datum_convert_wgs84_to_mean_sea_level`: synchronous stateless inverse batch conversion.

`tools/list` supplies detailed descriptions plus strict input and output JSON Schemas. Usage statistics are deliberately excluded from MCP.

## Generation, build, and tests

```powershell
dotnet tool restore
dotnet restore EarthVerticalDatum.sln
dotnet build Service/Service.csproj -c Release --no-restore
dotnet swagger tofile --output ModelSharedOut/json-schemas/EarthVerticalDatumFullName.json Service/bin/Release/net8.0/Service.dll v1
dotnet run --project ModelSharedOut/ModelSharedOut.csproj -c Release
dotnet build EarthVerticalDatum.sln -c Release --no-restore
dotnet test EarthVerticalDatum.sln -c Release --no-build
```

Generated artifacts are committed. CI regenerates them and fails when a contract change has not been checked in.

## Containers and Kubernetes

The images are `digiwells/osdcdrillingearthverticaldatumservice` and `digiwells/osdcdrillingearthverticaldatumwebappclient`. The Docker workflow uses private repository secrets `DOCKERHUB_USERNAME` and `DOCKERHUB_TOKEN`, then publishes stable/latest/SHA/version tags as applicable.

```powershell
docker build -f Service/Dockerfile -t digiwells/osdcdrillingearthverticaldatumservice:local .
docker build -f WebApp/Dockerfile -t digiwells/osdcdrillingearthverticaldatumwebappclient:local .
helm upgrade --install osdcearthverticaldatumservice Service/charts/osdcdrillingearthverticaldatumservice
helm upgrade --install osdcearthverticaldatumwebapp WebApp/charts/osdcdrillingearthverticaldatumwebappclient
```

The charts pull from DigiWells Docker Hub, create no PodDisruptionBudget, and require no persistence volume. Override hosts, tags, pull secrets, resources, probes, and autoscaling as needed.

## Publishing WebPages

`WebPages` packages as `OSDC.Drilling.EarthVerticalDatum.WebPages`. The NuGet workflow uses the private repository secret `NUGET_API_KEY`; trigger it manually with a version or push a `webpages-v*` tag.

Project code is MIT licensed. EGM84 grid and GeographicLib attribution is in [THIRD-PARTY-NOTICES.md](THIRD-PARTY-NOTICES.md).

Author: Eric Cayeux

Company: NORCE Research
