using GeographicLib;
using NUnit.Framework;
using OSDC.Drilling.EarthVerticalDatum.Model;

namespace OSDC.Drilling.EarthVerticalDatum.ModelTest;

public class Tests
{
    private EarthVerticalDatumEvaluator evaluator_ = null!;

    [SetUp]
    public void Setup() => evaluator_ = new EarthVerticalDatumEvaluator();

    [TearDown]
    public void TearDown() => evaluator_.Dispose();

    [Test]
    public void ConversionChangesSignsOnlyAtTheGeographicLibBoundary()
    {
        const double latitude = 0.7;
        const double longitude = -1.2;
        const double meanSeaLevelDepth = 1250;
        MeanSeaLevelToWgs84Response response = evaluator_.ConvertMeanSeaLevelToWgs84(
            Request(latitude, longitude, meanSeaLevelDepth));

        using var geoid = new Geoid("egm84-30", Path.Combine(AppContext.BaseDirectory, "VerticalDatumModelFiles"));
        double ellipsoidalHeight = geoid.ConvertHeight(latitude * 180 / Math.PI, longitude * 180 / Math.PI,
            -meanSeaLevelDepth, ConvertFlag.GeoidToEllipsoid);
        double expectedDepth = -ellipsoidalHeight;

        Assert.Multiple(() =>
        {
            Assert.That(response.Samples, Has.Count.EqualTo(1));
            Assert.That(response.Samples[0].Wgs84EllipsoidalDepth, Is.EqualTo(expectedDepth).Within(1e-12));
            Assert.That(response.Samples[0].GeoidUndulation,
                Is.EqualTo(meanSeaLevelDepth - expectedDepth).Within(1e-12));
            Assert.That(response.Samples[0].Position.MeanSeaLevelDepth, Is.EqualTo(meanSeaLevelDepth));
        });
    }

    [Test]
    public void InverseConversionChangesSignsOnlyAtTheGeographicLibBoundary()
    {
        const double latitude = 0.7;
        const double longitude = -1.2;
        const double wgs84EllipsoidalDepth = 1225;
        Wgs84ToMeanSeaLevelResponse response = evaluator_.ConvertWgs84ToMeanSeaLevel(
            InverseRequest(latitude, longitude, wgs84EllipsoidalDepth));

        using var geoid = new Geoid("egm84-30", Path.Combine(AppContext.BaseDirectory, "VerticalDatumModelFiles"));
        double orthometricHeight = geoid.ConvertHeight(latitude * 180 / Math.PI, longitude * 180 / Math.PI,
            -wgs84EllipsoidalDepth, ConvertFlag.EllipsoidToGeoid);
        double expectedDepth = -orthometricHeight;

        Assert.Multiple(() =>
        {
            Assert.That(response.Samples, Has.Count.EqualTo(1));
            Assert.That(response.Samples[0].MeanSeaLevelDepth, Is.EqualTo(expectedDepth).Within(1e-12));
            Assert.That(response.Samples[0].GeoidUndulation,
                Is.EqualTo(expectedDepth - wgs84EllipsoidalDepth).Within(1e-12));
            Assert.That(response.Samples[0].Position.Wgs84EllipsoidalDepth, Is.EqualTo(wgs84EllipsoidalDepth));
        });
    }

    [Test]
    public void ForwardAndInverseConversionsRoundTrip()
    {
        const double latitude = 1.1;
        const double longitude = 0.3;
        const double originalMeanSeaLevelDepth = 987.654;
        MeanSeaLevelToWgs84Response forward = evaluator_.ConvertMeanSeaLevelToWgs84(
            Request(latitude, longitude, originalMeanSeaLevelDepth));
        Wgs84ToMeanSeaLevelResponse inverse = evaluator_.ConvertWgs84ToMeanSeaLevel(
            InverseRequest(latitude, longitude, forward.Samples[0].Wgs84EllipsoidalDepth));

        Assert.That(inverse.Samples[0].MeanSeaLevelDepth,
            Is.EqualTo(originalMeanSeaLevelDepth).Within(1e-10));
    }

    [Test]
    public void ModelInformationIsTraceableAndThreadSafe()
    {
        Assert.Multiple(() =>
        {
            Assert.That(evaluator_.ModelInfo.ID, Is.EqualTo("EGM84-30"));
            Assert.That(evaluator_.ModelInfo.GridResolutionMinutes, Is.EqualTo(30));
            Assert.That(evaluator_.ModelInfo.Interpolation, Does.Contain("cubic").IgnoreCase);
            Assert.That(evaluator_.ModelInfo.ReferenceEllipsoid, Is.EqualTo("WGS84"));
            Assert.That(evaluator_.ModelInfo.DepthPositiveDirection, Is.EqualTo("down"));
            Assert.That(evaluator_.ModelInfo.IsThreadSafe, Is.True);
            Assert.That(evaluator_.ModelInfo.CoefficientSHA256, Has.Length.EqualTo(64));
        });
    }

    [Test]
    public void ThreadSafeModelSupportsConcurrentConversions()
    {
        MeanSeaLevelToWgs84Response[] responses = Enumerable.Range(0, 32).AsParallel()
            .Select(index => evaluator_.ConvertMeanSeaLevelToWgs84(Request(0.5, 1.0, index)))
            .ToArray();
        Assert.That(responses, Has.All.Property("Samples").Count.EqualTo(1));
    }

    [TestCase(Math.PI, 0, 0, "Latitude")]
    [TestCase(0, 4, 0, "Longitude")]
    [TestCase(0, 0, double.NaN, "MeanSeaLevelDepth")]
    public void InvalidPositionRejectsCompleteRequest(double latitude, double longitude, double depth, string property)
    {
        EarthVerticalDatumValidationException exception = Assert.Throws<EarthVerticalDatumValidationException>(
            () => evaluator_.ConvertMeanSeaLevelToWgs84(Request(latitude, longitude, depth)))!;
        Assert.That(exception.Errors, Has.Some.Property("Property").EqualTo(property));
    }

    [Test]
    public void MaximumBatchSizeIsEnforced()
    {
        var request = new MeanSeaLevelToWgs84Request { Positions = [new(), new()] };
        EarthVerticalDatumValidationException exception = Assert.Throws<EarthVerticalDatumValidationException>(
            () => evaluator_.ConvertMeanSeaLevelToWgs84(request, 1))!;
        Assert.That(exception.Errors, Has.Some.Property("Code").EqualTo("too_many"));
    }

    [Test]
    public void InvalidInverseDepthRejectsCompleteRequest()
    {
        EarthVerticalDatumValidationException exception = Assert.Throws<EarthVerticalDatumValidationException>(
            () => evaluator_.ConvertWgs84ToMeanSeaLevel(InverseRequest(0, 0, double.NaN)))!;
        Assert.That(exception.Errors, Has.Some.Property("Property").EqualTo("Wgs84EllipsoidalDepth"));
    }

    private static MeanSeaLevelToWgs84Request Request(double latitude, double longitude, double depth) => new()
    {
        Positions =
        [
            new EarthVerticalDatumPosition
            {
                Latitude = latitude,
                Longitude = longitude,
                MeanSeaLevelDepth = depth
            }
        ]
    };

    private static Wgs84ToMeanSeaLevelRequest InverseRequest(double latitude, double longitude, double depth) => new()
    {
        Positions =
        [
            new Wgs84ToMeanSeaLevelPosition
            {
                Latitude = latitude,
                Longitude = longitude,
                Wgs84EllipsoidalDepth = depth
            }
        ]
    };
}
