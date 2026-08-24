namespace OSDC.Drilling.EarthVerticalDatum.Model;

public sealed class EarthVerticalDatumValidationException(IReadOnlyList<EarthVerticalDatumValidationError> errors)
    : Exception("The Earth vertical datum conversion request is invalid.")
{
    public IReadOnlyList<EarthVerticalDatumValidationError> Errors { get; } = errors;
}
