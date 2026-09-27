# ServiceTest

`ServiceTest` hosts the real ASP.NET Core application in memory and verifies both generated-client conversion directions, structured HTTP 422 validation, canonical and case-insensitive discovery routes, health/metrics/schema endpoints, exact MCP registration, both MCP conversion calls, detailed `tools/list` schemas, exclusion of usage statistics from MCP, and restoration of persisted counters after a service restart. MCP regression coverage verifies structured and backward-compatible JSON text results under protocol `2025-03-26`, RFC 3339 UTC model timestamps, and the reported 55-position conversion payload.

Run with `dotnet test ServiceTest/ServiceTest.csproj`.

Author: Eric Cayeux

Company: NORCE Research

Semantic contract tests compare model attributes with live REST, committed merged OpenAPI, and both MCP conversion schemas. They verify catalogue 0.4.0, curated status, quantities, grid spacing in radians, removal of the legacy field, and the schema server URL beneath the API PathBase.
