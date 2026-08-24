using Microsoft.AspNetCore.HttpOverrides;
using MudBlazor;
using MudBlazor.Services;
using OSDC.Drilling.EarthVerticalDatum.WebApp;
using OSDC.Drilling.EarthVerticalDatum.WebPages;

var builder = WebApplication.CreateBuilder(args);
var webPagesConfiguration = new WebPagesHostConfiguration
{
    EarthVerticalDatumHostURL = builder.Configuration["EarthVerticalDatumHostURL"] ?? string.Empty,
    UnitConversionHostURL = builder.Configuration["UnitConversionHostURL"] ?? string.Empty
};

builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddMudServices(configuration =>
{
    configuration.SnackbarConfiguration.PositionClass = Defaults.Classes.Position.BottomLeft;
    configuration.SnackbarConfiguration.PreventDuplicates = false;
    configuration.SnackbarConfiguration.NewestOnTop = false;
    configuration.SnackbarConfiguration.ShowCloseIcon = true;
    configuration.SnackbarConfiguration.SnackbarVariant = Variant.Filled;
});
builder.Services.AddSingleton<IEarthVerticalDatumWebPagesConfiguration>(webPagesConfiguration);
builder.Services.AddSingleton<IEarthVerticalDatumAPIUtils, APIUtils>();

var app = builder.Build();
var forwardedHeaders = new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
};
forwardedHeaders.KnownNetworks.Clear();
forwardedHeaders.KnownProxies.Clear();
app.UseForwardedHeaders(forwardedHeaders);
app.UsePathBase("/EarthVerticalDatum/webapp");

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}
app.UseStaticFiles();
app.UseRouting();
app.MapGet("/EarthVerticalDatum", () => Results.Redirect("/EarthVerticalDatum/webapp/Home"))
    .ExcludeFromDescription();
app.MapGet("/health/live", () => Results.Ok(new { Status = "Healthy" })).ExcludeFromDescription();
app.MapGet("/health/ready", () => Results.Ok(new { Status = "Healthy" })).ExcludeFromDescription();
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");
app.Run();

public partial class Program;
