using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using OSDC.Drilling.EarthVerticalDatum.Model;

namespace OSDC.Drilling.EarthVerticalDatum.Service.Controllers;

[Produces("application/json")]
[Route("[controller]")]
[ApiController]
public class EarthVerticalDatumController(
    EarthVerticalDatumEvaluator evaluator,
    UsageStatisticsEarthVerticalDatum statistics,
    IOptions<EarthVerticalDatumServiceOptions> options) : ControllerBase
{
    /// <summary>Returns the loaded EGM84-30 geoid-model information for service discovery.</summary>
    /// <remarks>This is the microservice entry endpoint. It returns the same model identity and provenance as the ModelInfo endpoint.</remarks>
    [HttpGet(Name = "GetEarthVerticalDatumEntry")]
    public ActionResult<EarthVerticalDatumModelInfo> GetEarthVerticalDatumEntry() => GetModelInfoResponse();

    /// <summary>Synchronously converts EGM84 mean-sea-level depths to WGS84 ellipsoidal depths.</summary>
    /// <remarks>This operation is stateless. Latitude and longitude are WGS84 radians. Input and output depths are SI metres, positive downward from their explicitly named reference surfaces. The complete request is rejected if any position is invalid.</remarks>
    [HttpPost("ConvertMeanSeaLevelToWgs84", Name = "ConvertMeanSeaLevelToWgs84")]
    [ProducesResponseType(typeof(MeanSeaLevelToWgs84Response), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(EarthVerticalDatumValidationProblem), StatusCodes.Status422UnprocessableEntity)]
    public ActionResult<MeanSeaLevelToWgs84Response> ConvertMeanSeaLevelToWgs84(
        [FromBody] MeanSeaLevelToWgs84Request request, CancellationToken cancellationToken)
    {
        statistics.IncrementConversion(false, request?.Positions?.Count ?? 0);
        try
        {
            return Ok(evaluator.ConvertMeanSeaLevelToWgs84(request, options.Value.MaximumPositionsPerRequest, cancellationToken));
        }
        catch (EarthVerticalDatumValidationException exception)
        {
            statistics.IncrementFailedConversion();
            return UnprocessableEntity(new EarthVerticalDatumValidationProblem
            {
                Message = exception.Message,
                Errors = exception.Errors.ToList()
            });
        }
    }

    /// <summary>Synchronously converts WGS84 ellipsoidal depths to EGM84 mean-sea-level depths.</summary>
    /// <remarks>This inverse operation is stateless. Latitude and longitude are WGS84 radians. Input and output depths are SI metres, positive downward from their explicitly named reference surfaces. The complete request is rejected if any position is invalid.</remarks>
    [HttpPost("ConvertWgs84ToMeanSeaLevel", Name = "ConvertWgs84ToMeanSeaLevel")]
    [ProducesResponseType(typeof(Wgs84ToMeanSeaLevelResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(EarthVerticalDatumValidationProblem), StatusCodes.Status422UnprocessableEntity)]
    public ActionResult<Wgs84ToMeanSeaLevelResponse> ConvertWgs84ToMeanSeaLevel(
        [FromBody] Wgs84ToMeanSeaLevelRequest request, CancellationToken cancellationToken)
    {
        statistics.IncrementConversion(false, request?.Positions?.Count ?? 0);
        try
        {
            return Ok(evaluator.ConvertWgs84ToMeanSeaLevel(request, options.Value.MaximumPositionsPerRequest, cancellationToken));
        }
        catch (EarthVerticalDatumValidationException exception)
        {
            statistics.IncrementFailedConversion();
            return UnprocessableEntity(new EarthVerticalDatumValidationProblem
            {
                Message = exception.Message,
                Errors = exception.Errors.ToList()
            });
        }
    }

    /// <summary>Returns the loaded EGM84-30 geoid model identity, resolution, interpolation accuracy, runtime version, and coefficient hash.</summary>
    [HttpGet("ModelInfo", Name = "GetEarthVerticalDatumModelInfo")]
    public ActionResult<EarthVerticalDatumModelInfo> GetEarthVerticalDatumModelInfo() => GetModelInfoResponse();

    private ActionResult<EarthVerticalDatumModelInfo> GetModelInfoResponse()
    {
        statistics.IncrementModelInfo();
        return Ok(evaluator.ModelInfo);
    }
}
