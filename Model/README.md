# Model

`Model` contains the public vertical-datum contracts, atomic request validation, thread-safe cumulative usage counters, and the stateless `EarthVerticalDatumEvaluator`. The service can restore counter totals and their original start time from its private JSON snapshot.

The evaluator loads GeographicLib `egm84-30` once with cubic interpolation and thread-safe mode. It converts in both directions between EGM84 mean-sea-level depth and WGS84 ellipsoidal depth. Public coordinates use radians and both depths use metres positive downward. GeographicLib degree and positive-up conversions are confined to the implementation boundary.

`EarthVerticalDatumModelInfo` exposes model identity, grid resolution, interpolation, published maximum/RMS errors, the model data timestamp marked as UTC for RFC 3339 serialization, GeographicLib version, reference surfaces, depth direction, thread-safety, and grid SHA-256.

Validation rejects the entire batch for missing/empty/oversized input, non-finite values, or coordinates outside WGS84 ranges. Results preserve input order. The project copies `../VerticalDatumModelFiles` into build and publish output.

Author: Eric Cayeux

Company: NORCE Research

Model-owned `Semantic` attributes bind the curated SemanticCatalogue 0.4.0 vocabulary to both conversion directions and model provenance. AngularGridSpacing is in SI radians; geoid undulation uses LengthStandard (1 mm meaningful display precision), depths use DepthDrilling, and representation errors use Length. Precision is not model accuracy and does not round API values.
