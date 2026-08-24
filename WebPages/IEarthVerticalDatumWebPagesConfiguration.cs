using OSDC.DotnetLibraries.Drilling.WebAppUtils;

namespace OSDC.Drilling.EarthVerticalDatum.WebPages;

public interface IEarthVerticalDatumWebPagesConfiguration : IUnitConversionHostURL
{
    string EarthVerticalDatumHostURL { get; }
}
