using MiniPdf.Web.Components;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<MiniPdf.Web.Client.I18n>();
builder.Services.AddRazorComponents()
    .AddInteractiveWebAssemblyComponents();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
}

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(MiniPdf.Web.Client._Imports).Assembly);

app.Run();
