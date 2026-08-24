# OSDC.Drilling.EarthVerticalDatum.WebPages

Reusable Blazor pages for the stateless OSDC Earth Vertical Datum service.

- `/EarthVerticalDatumCalculation`: unit-aware conversion from EGM84 mean-sea-level depth to WGS84 ellipsoidal depth.
- `/EarthVerticalDatumModel`: model identity, interpolation accuracy, conventions, runtime, thread-safety, and grid hash.
- `/StatisticsEarthVerticalDatum`: process-replica REST usage counters.

The package compiles the generated client from `ModelSharedOut`. The consuming application must register an `HttpClient` named `EarthVerticalDatumHostURL`, MudBlazor, and the OSDC unit-system services used by the controls.

Package ID: `OSDC.Drilling.EarthVerticalDatum.WebPages`

Author: Eric Cayeux

Company: NORCE Research
