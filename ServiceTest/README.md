# ServiceTest

`ServiceTest` hosts the real ASP.NET Core application in memory and verifies both generated-client conversion directions, structured HTTP 422 validation, canonical and case-insensitive discovery routes, health/metrics/schema endpoints, exact MCP registration, both MCP conversion calls, detailed `tools/list` schemas, exclusion of usage statistics from MCP, and restoration of persisted counters after a service restart.

Run with `dotnet test ServiceTest/ServiceTest.csproj`.

Author: Eric Cayeux

Company: NORCE Research
