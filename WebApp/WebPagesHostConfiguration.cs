using OSDC.Drilling.EarthVerticalDatum.WebPages;

namespace OSDC.Drilling.EarthVerticalDatum.WebApp;

public class WebPagesHostConfiguration : IEarthVerticalDatumWebPagesConfiguration
{
    public string EarthVerticalDatumHostURL { get; set; } = string.Empty;
    public string? UnitConversionHostURL { get; set; } = string.Empty;
}
