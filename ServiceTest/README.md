# ServiceTest

`ServiceTest` hosts the real ASP.NET Core application in memory and verifies generated-client conversion, structured HTTP 422 validation, canonical and case-insensitive discovery routes, health/metrics/schema endpoints, exact MCP registration, detailed `tools/list` schemas, and exclusion of usage statistics from MCP.

Run with `dotnet test ServiceTest/ServiceTest.csproj`.

Author: Eric Cayeux

Company: NORCE Research
