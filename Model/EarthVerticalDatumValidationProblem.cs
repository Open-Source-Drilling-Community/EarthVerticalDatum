namespace OSDC.Drilling.EarthVerticalDatum.Model;

/// <summary>Details returned when a conversion request is rejected atomically.</summary>
public class EarthVerticalDatumValidationProblem
{
    public string Error { get; set; } = "invalid_request";
    public string Message { get; set; } = string.Empty;
    public List<EarthVerticalDatumValidationError> Errors { get; set; } = [];
}
