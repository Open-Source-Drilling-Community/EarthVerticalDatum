# OSDC.Drilling.EarthVerticalDatum.WebPages

This release targets MudBlazor 9.9.0 and the matching OSDC shared web component packages.

Reusable Blazor pages for the stateless OSDC Earth Vertical Datum service.

- `/EarthVerticalDatumCalculation`: selectable unit-aware conversion in both directions between EGM84 mean-sea-level and WGS84 ellipsoidal depths.
- `/EarthVerticalDatumModel`: model identity, interpolation accuracy, conventions, runtime, thread-safety, and grid hash.
- `/StatisticsEarthVerticalDatum`: cumulative REST and MCP usage counters retained by the service across restarts.

The package compiles the generated client from `ModelSharedOut`. The consuming application must register an `HttpClient` named `EarthVerticalDatumHostURL`, MudBlazor, and the OSDC unit-system services used by the controls.

Package ID: `OSDC.Drilling.EarthVerticalDatum.WebPages`

Author: Eric Cayeux

Company: NORCE Research

The calculator displays geoid undulation through LengthStandard (1 mm meaningful display precision). AngularGridSpacing uses PlaneAngleGeodesic; model representation errors use Length. All unit-bearing values follow the selected unit system while REST values remain SI.
