# WebApp

`WebApp` is the server-side Blazor host for `WebPages`. It is served below `/EarthVerticalDatum/webapp`; `/EarthVerticalDatum/webapp/EarthVerticalDatum` redirects to `/EarthVerticalDatum/webapp/Home` for OSDC discovery-page compatibility.

Development calls the Service at `http://localhost:58948/`; production defaults to Kubernetes DNS `http://osdcearthverticaldatumservice/`. The external Unit Conversion service supplies the UI unit-reference system.

Run `dotnet run --project WebApp/WebApp.csproj`, then browse to `http://localhost:58950/EarthVerticalDatum/webapp/Home`. The image is `digiwells/osdcdrillingearthverticaldatumwebappclient`; its chart is under `charts/osdcdrillingearthverticaldatumwebappclient`.

Author: Eric Cayeux

Company: NORCE Research
