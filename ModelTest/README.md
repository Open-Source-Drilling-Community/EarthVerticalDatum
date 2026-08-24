# ModelTest

`ModelTest` verifies both conversion directions against direct GeographicLib EGM84-30 calls, forward/inverse round trips, model provenance and grid hash, thread-safe concurrent conversion, WGS84 coordinate bounds, finite depth validation, and the configured batch limit. Required EGM84 files are copied to test output by `ModelTest.csproj`.

Run with `dotnet test ModelTest/ModelTest.csproj`.

Author: Eric Cayeux

Company: NORCE Research
