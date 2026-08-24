namespace OSDC.Drilling.EarthVerticalDatum.Model;

public record EarthVerticalDatumValidationError(int? PositionIndex, string Property, string Code, string Message);
