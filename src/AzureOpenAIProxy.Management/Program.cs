using AzureOpenAIProxy.Management;
using AzureOpenAIProxy.Management.Components;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Web;
using MudBlazor.Services;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.AddAuth();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddMicrosoftIdentityConsentHandler();

builder.Services.AddDbContext<AoaiProxyContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("AoaiProxyContext"));
});

builder.Services.AddMudServices();

WebApplication app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.UseAntiforgery();

app.UseStaticFiles();

app.MapControllers();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
