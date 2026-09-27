# Semantic bindings

The Model references published `OSDC.DotnetLibraries.Drilling.SemanticCatalogue` 0.4.0 unconditionally. Model attributes own the bindings; REST/OpenAPI and MCP publish the same curated `x-osdc-semantic` metadata. Generated clients retain SI wire values.

| Field | Concept / physical quantity | Convention |
| --- | --- | --- |
| Latitude, Longitude | Latitude, Longitude / PlaneAngleGeodesic | WGS84 radians |
| MeanSeaLevelDepth | GeoidReferencedDepth / DepthDrilling | Metres, positive down from EGM84 geoid |
| Wgs84EllipsoidalDepth | EllipsoidalDepth / DepthDrilling | Metres, positive down from WGS84 ellipsoid |
| GeoidUndulation | GeoidUndulation / LengthStandard | Metres, positive up from ellipsoid to geoid |
| AngularGridSpacing | AngularGridSpacing / PlaneAngleGeodesic | Radians, pi/360 for this grid |
| MaximumInterpolationError | GeoidApproximationError / Length, MaximumAbsoluteError role | Metres |
| RMSInterpolationError | GeoidApproximationError / Length, RootMeanSquareError role | Metres; not standard deviation |
| DataDateTime | Instant, DatasetTimestamp role | UTC timestamp from grid header |
| CoefficientSHA256 | Sha256FileDigest, CoefficientFile role | SHA-256 of complete PGM bytes |

Geoid undulation requires both the EGM84 geoid and WGS84 ellipsoid; provider descriptions and model provenance identify that pair. EGM84 approximates mean sea level, not a local tidal datum or instantaneous sea surface. For positive-down depths, `D_wgs84 = D_geoid - N` and `D_geoid = D_wgs84 + N`.

LengthStandard gives geoid undulation 1 mm meaningful display precision. This neither rounds calculation/API values nor asserts geoid accuracy. Reported maximum and RMS errors describe quantization and interpolation relative to the reference geoid, not total Earth-model accuracy.

Requests, responses, samples, positions and model provenance have curated concepts. Input/output collections and provenance have distinct roles. Provider facts without a matching curated noun remain explicitly described, including supported directions, thread safety and depth sign convention.

`GridResolutionMinutes` is removed without an alias. Consumers must use `AngularGridSpacing` in radians and regenerate clients. UI values use the selected display units. The four MCP tools and stateless conversion behavior are unchanged.
