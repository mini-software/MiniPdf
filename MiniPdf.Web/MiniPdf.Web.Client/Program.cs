using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.JSInterop;
using MiniPdf.Web.Client;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");

builder.Services.AddSingleton<I18n>();

var host = builder.Build();

// Restore saved language from localStorage
var js = host.Services.GetRequiredService<IJSRuntime>();
var i18n = host.Services.GetRequiredService<I18n>();
try
{
    var saved = await js.InvokeAsync<string?>("localStorage.getItem", "minipdf-lang");
    if (!string.IsNullOrEmpty(saved))
        i18n.SetLanguage(saved);
    else
    {
        // Detect browser language
        var browserLang = await js.InvokeAsync<string>("eval", "navigator.language || navigator.userLanguage || 'en'");
        if (!string.IsNullOrEmpty(browserLang))
        {
            // Try exact match first, then prefix match
            var match = I18n.SupportedLanguages.FirstOrDefault(l =>
                string.Equals(l.Code, browserLang, StringComparison.OrdinalIgnoreCase));
            if (match is null)
            {
                var prefix = browserLang.Split('-')[0].ToLowerInvariant();
                match = I18n.SupportedLanguages.FirstOrDefault(l =>
                    l.Code.Split('-')[0].Equals(prefix, StringComparison.OrdinalIgnoreCase));
            }
            if (match is not null)
                i18n.SetLanguage(match.Code);
        }
    }
}
catch { /* localStorage or JS not available — keep default */ }

await host.RunAsync();
