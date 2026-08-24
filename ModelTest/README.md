# ModelTest

`ModelTest` verifies signs and values against a direct GeographicLib EGM84-30 call, model provenance and grid hash, thread-safe concurrent conversion, WGS84 coordinate bounds, finite depth validation, and the configured batch limit. Required EGM84 files are copied to test output by `ModelTest.csproj`.

Run with `dotnet test ModelTest/ModelTest.csproj`.

Author: Eric Cayeux

Company: NORCE Research
