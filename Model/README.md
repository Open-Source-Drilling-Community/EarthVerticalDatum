# Model

`Model` contains the public vertical-datum contracts, atomic request validation, in-process usage counters, and the stateless `EarthVerticalDatumEvaluator`.

The evaluator loads GeographicLib `egm84-30` once with cubic interpolation and thread-safe mode. It accepts WGS84 latitude/longitude in radians and EGM84 mean-sea-level depth in metres positive downward, then returns WGS84 ellipsoidal depth in metres positive downward. GeographicLib degree and positive-up conversions are confined to the implementation boundary.

`EarthVerticalDatumModelInfo` exposes model identity, grid resolution, interpolation, published maximum/RMS errors, data timestamp, GeographicLib version, reference surfaces, depth direction, thread-safety, and grid SHA-256.

Validation rejects the entire batch for missing/empty/oversized input, non-finite values, or coordinates outside WGS84 ranges. Results preserve input order. The project copies `../VerticalDatumModelFiles` into build and publish output.

Author: Eric Cayeux

Company: NORCE Research
