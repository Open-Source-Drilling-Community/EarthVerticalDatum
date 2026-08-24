using OSDC.Drilling.EarthVerticalDatum.ModelShared;

namespace OSDC.Drilling.EarthVerticalDatum.WebPages;

public class APIUtils : OSDC.DotnetLibraries.Drilling.WebAppUtils.APIUtils, IEarthVerticalDatumAPIUtils
{
    public APIUtils(IEarthVerticalDatumWebPagesConfiguration configuration)
    {
        HostNameEarthVerticalDatum = Require(configuration.EarthVerticalDatumHostURL, nameof(configuration.EarthVerticalDatumHostURL));
        HttpClientEarthVerticalDatum = SetHttpClient(HostNameEarthVerticalDatum, HostBasePathEarthVerticalDatum);
        ClientEarthVerticalDatum = new Client(HttpClientEarthVerticalDatum.BaseAddress!.ToString(), HttpClientEarthVerticalDatum);
        HostNameUnitConversion = Require(configuration.UnitConversionHostURL, nameof(configuration.UnitConversionHostURL));
    }

    public string HostNameEarthVerticalDatum { get; }
    public string HostBasePathEarthVerticalDatum { get; } = "EarthVerticalDatum/api/";
    public HttpClient HttpClientEarthVerticalDatum { get; }
    public Client ClientEarthVerticalDatum { get; }
    public string HostNameUnitConversion { get; }
    public string HostBasePathUnitConversion { get; } = "UnitConversion/api/";

    private static string Require(string? value, string property)
    {
        if (string.IsNullOrWhiteSpace(value)) throw new InvalidOperationException($"Configuration value '{property}' is required.");
        return value;
    }
}
