# OSDC.Drilling.EarthVerticalDatum.WebPages

Reusable Blazor pages for the stateless OSDC Earth Vertical Datum service.

- `/EarthVerticalDatumCalculation`: selectable unit-aware conversion in both directions between EGM84 mean-sea-level and WGS84 ellipsoidal depths.
- `/EarthVerticalDatumModel`: model identity, interpolation accuracy, conventions, runtime, thread-safety, and grid hash.
- `/StatisticsEarthVerticalDatum`: cumulative REST and MCP usage counters retained by the service across restarts.

The package compiles the generated client from `ModelSharedOut`. The consuming application must register an `HttpClient` named `EarthVerticalDatumHostURL`, MudBlazor, and the OSDC unit-system services used by the controls.

Package ID: `OSDC.Drilling.EarthVerticalDatum.WebPages`

Author: Eric Cayeux

Company: NORCE Research
