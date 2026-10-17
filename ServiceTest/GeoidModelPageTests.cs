extern alias DatumWebPages;

using System.Net;
using System.Text;
using System.Text.Json;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor.Services;
using NUnit.Framework;
using OSDC.UnitConversion.DrillingRazorMudComponents;
using GeoidPage = DatumWebPages::OSDC.Drilling.EarthVerticalDatum.WebPages.EarthVerticalDatumModel;
using PageApi = DatumWebPages::OSDC.Drilling.EarthVerticalDatum.WebPages.IEarthVerticalDatumAPIUtils;
using PageClient = DatumWebPages::OSDC.Drilling.EarthVerticalDatum.ModelShared.Client;

namespace OSDC.Drilling.EarthVerticalDatum.ServiceTest;

public class GeoidModelPageTests
{
    [Test]
    public void LoadedGeoidModelRendersItsGridResolutionAsReadOnly()
    {
        using var context = new Bunit.TestContext();
        context.Services.AddMudServices();
        using var http = new HttpClient(new ModelInfoHandler());
        context.Services.AddSingleton<PageApi>(new ModelInfoApi(http));
        context.JSInterop.Mode = JSRuntimeMode.Loose;
        // Keep the real page and unit field; replace only the selector's remote discovery.
        var units = new MudUnitAndReferenceChoiceTag();
        context.ComponentFactories.AddStub<MudUnitAndReferenceChoiceTag>(parameters => builder =>
        {
            builder.OpenComponent<CascadingValue<MudUnitAndReferenceChoiceTag>>(0);
            builder.AddAttribute(1, "Value", units);
            builder.AddAttribute(2, "ChildContent", parameters.Get(p => p.ChildContent));
            builder.CloseComponent();
        });
        var page = context.RenderComponent<GeoidPage>();
        page.WaitForAssertion(() =>
        {
            Assert.That(page.Markup, Does.Contain("EGM84-30"));
            var grid = page.FindComponent<MudInputWithUnitAdornment>();
            Assert.Multiple(() =>
            {
                Assert.That(grid.Instance.ReadOnly, Is.True);
                Assert.That(grid.Instance.QuantityName, Is.EqualTo("PlaneAngleGeodesic"));
                Assert.That(grid.Instance.SIValueNullable, Is.EqualTo(Math.PI / 360));
                Assert.That(grid.Find("input").HasAttribute("readonly"), Is.True);
            });
        });
    }

    private sealed class ModelInfoHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var model = new Model.EarthVerticalDatumModelInfo
            {
                Name = "egm84-30", ID = "EGM84-30", AngularGridSpacing = Math.PI / 360,
                Description = "Geoid model rendering regression fixture"
            };
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonSerializer.Serialize(model), Encoding.UTF8, "application/json")
            });
        }
    }

    private sealed class ModelInfoApi(HttpClient http) : PageApi
    {
        public string HostNameEarthVerticalDatum => "http://datum.test/";
        public string HostBasePathEarthVerticalDatum => "EarthVerticalDatum/api/";
        public HttpClient HttpClientEarthVerticalDatum => http;
        public PageClient ClientEarthVerticalDatum { get; } = new("http://datum.test/EarthVerticalDatum/api/", http);
        public string HostNameUnitConversion => string.Empty;
        public string HostBasePathUnitConversion => string.Empty;
    }
}
