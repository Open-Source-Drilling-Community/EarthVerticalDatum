using System.Globalization;
using System.Reflection;
using System.Security.Cryptography;
using GeographicLib;

namespace OSDC.Drilling.EarthVerticalDatum.Model;

/// <summary>Loads EGM84-30 once and performs stateless conversions using OSDC SI and positive-down conventions.</summary>
public sealed class EarthVerticalDatumEvaluator : IDisposable
{
    private const string GeoidName = "egm84-30";
    private readonly Geoid geoidModel_;

    public EarthVerticalDatumEvaluator(string? modelDirectory = null)
    {
        string directory = ResolveModelDirectory(modelDirectory);
        string coefficientPath = Path.Combine(directory, GeoidName + ".pgm");
        if (!File.Exists(coefficientPath))
            throw new FileNotFoundException($"Required EGM84-30 geoid file was not found in '{directory}'.", coefficientPath);

        // Cubic interpolation is explicit. Thread-safe mode loads the small grid into memory,
        // closes the file, and disables GeographicLib's mutable single-cell cache.
        geoidModel_ = new Geoid(GeoidName, directory, cubic: true, threadsafe: true);
        ModelInfo = new EarthVerticalDatumModelInfo
        {
            Name = geoidModel_.GeoidName,
            ID = "EGM84-30",
            Description = geoidModel_.Description,
            DataDateTime = geoidModel_.DateTime,
            GridResolutionMinutes = 30,
            Interpolation = geoidModel_.Interpolation,
            MaximumInterpolationError = geoidModel_.MaxError,
            RMSInterpolationError = geoidModel_.RMSError,
            GeographicLibVersion = typeof(Geoid).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion
                ?? typeof(Geoid).Assembly.GetName().Version?.ToString() ?? "Unknown",
            IsThreadSafe = geoidModel_.IsThreadSafe,
            CoefficientSHA256 = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(coefficientPath))).ToLowerInvariant()
        };
    }

    public EarthVerticalDatumModelInfo ModelInfo { get; }

    public MeanSeaLevelToWgs84Response ConvertMeanSeaLevelToWgs84(MeanSeaLevelToWgs84Request? request,
        int maximumPositions = 10_000, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<EarthVerticalDatumValidationError> errors = Validate(request, maximumPositions);
        if (errors.Count != 0) throw new EarthVerticalDatumValidationException(errors);

        var response = new MeanSeaLevelToWgs84Response { Model = ModelInfo };
        foreach (EarthVerticalDatumPosition position in request!.Positions)
        {
            cancellationToken.ThrowIfCancellationRequested();
            double latitudeDegrees = position.Latitude * 180.0 / Math.PI;
            double longitudeDegrees = position.Longitude * 180.0 / Math.PI;

            // GeographicLib uses conventional heights positive upward. The OSDC API uses
            // depths positive downward, so signs are changed only at this library boundary.
            double orthometricHeight = -position.MeanSeaLevelDepth;
            double ellipsoidalHeight = geoidModel_.ConvertHeight(
                latitudeDegrees, longitudeDegrees, orthometricHeight, ConvertFlag.GeoidToEllipsoid);
            double wgs84EllipsoidalDepth = -ellipsoidalHeight;

            response.Samples.Add(new EarthVerticalDatumSample
            {
                Position = new EarthVerticalDatumPosition
                {
                    Latitude = position.Latitude,
                    Longitude = position.Longitude,
                    MeanSeaLevelDepth = position.MeanSeaLevelDepth
                },
                Wgs84EllipsoidalDepth = wgs84EllipsoidalDepth,
                GeoidUndulation = position.MeanSeaLevelDepth - wgs84EllipsoidalDepth
            });
        }
        return response;
    }

    public void Dispose() => geoidModel_.Dispose();

    private static IReadOnlyList<EarthVerticalDatumValidationError> Validate(
        MeanSeaLevelToWgs84Request? request, int maximumPositions)
    {
        var errors = new List<EarthVerticalDatumValidationError>();
        if (request?.Positions == null)
        {
            errors.Add(new(null, "Positions", "required", "Positions is required."));
            return errors;
        }
        if (request.Positions.Count == 0)
            errors.Add(new(null, "Positions", "empty", "At least one position is required."));
        if (request.Positions.Count > maximumPositions)
            errors.Add(new(null, "Positions", "too_many",
                $"At most {maximumPositions.ToString(CultureInfo.InvariantCulture)} positions are allowed."));

        for (int index = 0; index < request.Positions.Count; index++)
        {
            EarthVerticalDatumPosition? position = request.Positions[index];
            if (position == null)
            {
                errors.Add(new(index, "Position", "required", "Position must not be null."));
                continue;
            }
            ValidateAngle(errors, index, "Latitude", position.Latitude, -Math.PI / 2, Math.PI / 2);
            ValidateAngle(errors, index, "Longitude", position.Longitude, -Math.PI, Math.PI);
            if (!double.IsFinite(position.MeanSeaLevelDepth))
                errors.Add(new(index, "MeanSeaLevelDepth", "not_finite",
                    "MeanSeaLevelDepth must be a finite value in SI metres."));
        }
        return errors;
    }

    private static void ValidateAngle(List<EarthVerticalDatumValidationError> errors, int index, string property,
        double value, double minimum, double maximum)
    {
        if (!double.IsFinite(value))
            errors.Add(new(index, property, "not_finite", $"{property} must be finite."));
        else if (value < minimum || value > maximum)
            errors.Add(new(index, property, "out_of_range",
                $"{property} must be between {minimum} and {maximum} SI radians."));
    }

    private static string ResolveModelDirectory(string? configured) => !string.IsNullOrWhiteSpace(configured)
        ? Path.GetFullPath(configured)
        : Path.Combine(AppContext.BaseDirectory, "VerticalDatumModelFiles");
}
