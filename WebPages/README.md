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

## Package release 1.1.4

This release packages the current WebPages and generated contracts. Update consuming WebApps to `OSDC.Drilling.EarthVerticalDatum.WebPages` version `1.1.4` after it is available on nuget.org.

## Trusted NuGet publishing

The `publish-webpages-nuget.yml` workflow authenticates with GitHub OIDC through `NuGet/login@v1`; no long-lived `NUGET_API_KEY` secret is used. Set `NUGET_USER` to the nuget.org profile username as a repository variable (a repository secret is also accepted). The publishing job requests `contents: read` and `id-token: write` and obtains its temporary credential immediately before uploading.

On nuget.org, the package owner must configure a trusted-publishing policy for `Open-Source-Drilling-Community/EarthVerticalDatum`, workflow filename `publish-webpages-nuget.yml`, with permission to push new versions of `OSDC.Drilling.EarthVerticalDatum.WebPages`. Leave the policy environment empty because the workflow does not declare a GitHub environment. Trigger the workflow manually with the intended version or push a `webpages-v*` tag. Already published versions are skipped.

## Package release 1.1.5

Requires OSDC.UnitConversion.DrillingRazorMudComponents 3.4.8 so embedded unit selectors consume bounded UnitSystem discovery pages. Rebuild consuming WebApps after upgrading this package.
