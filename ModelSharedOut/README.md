# ModelSharedOut

`ModelSharedOut` generates shared C# DTOs and a typed REST client from the Service OpenAPI document. Committed outputs are `json-schemas/EarthVerticalDatumFullName.json`, `EarthVerticalDatumMergedModel.cs`, and `../Service/wwwroot/json-schema/EarthVerticalDatumMergedModel.json`. `PseudoConstructors.cs` contains hand-maintained convenience constructors compiled into `WebPages`.

After changing a public REST contract, build `Service`, run `dotnet swagger tofile`, and execute this project. CI repeats generation and rejects an uncommitted difference.

Author: Eric Cayeux

Company: NORCE Research

Regenerated contracts carry SemanticCatalogue 0.4.0 annotations and expose only AngularGridSpacing in radians. Regenerate downstream clients for this intentional breaking field/unit change. The generated DTOs require no direct SemanticCatalogue reference.
