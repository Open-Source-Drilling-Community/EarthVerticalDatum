using Microsoft.AspNetCore.Mvc;
using OSDC.Drilling.EarthVerticalDatum.Model;

namespace OSDC.Drilling.EarthVerticalDatum.Service.Controllers;

[Produces("application/json")]
[Route("[controller]")]
[ApiController]
public class EarthVerticalDatumUsageStatisticsController(UsageStatisticsEarthVerticalDatum statistics) : ControllerBase
{
    /// <summary>Returns cumulative usage counters retained across service restarts. This operation is intentionally not exposed as an MCP tool.</summary>
    [HttpGet(Name = "GetEarthVerticalDatumUsageStatistics")]
    public ActionResult<UsageStatisticsEarthVerticalDatum> GetEarthVerticalDatumUsageStatistics()
    {
        statistics.IncrementStatistics();
        return Ok(statistics);
    }
}
