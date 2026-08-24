using OSDC.Drilling.EarthVerticalDatum.ModelShared;

namespace OSDC.Drilling.EarthVerticalDatum.WebPages;

public interface IEarthVerticalDatumAPIUtils
{
    string HostNameEarthVerticalDatum { get; }
    string HostBasePathEarthVerticalDatum { get; }
    HttpClient HttpClientEarthVerticalDatum { get; }
    Client ClientEarthVerticalDatum { get; }
    string HostNameUnitConversion { get; }
    string HostBasePathUnitConversion { get; }
}
