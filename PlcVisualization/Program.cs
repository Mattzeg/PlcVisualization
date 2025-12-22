using PlcVisualization.Components;
using PlcVisualization.Services;
using PlcVisualization.Hubs;
using PlcVisualization.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// SignalR für Real-time Updates
builder.Services.AddSignalR();

// PLC Settings aus appsettings.json laden
builder.Services.Configure<PlcSettings>(builder.Configuration.GetSection("PlcSettings"));

// PLC Service als Singleton und HostedService
builder.Services.AddSingleton<PlcService>();
builder.Services.AddHostedService(sp => sp.GetRequiredService<PlcService>());

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// SignalR Hub für Antriebsdaten
app.MapHub<DriveHub>("/driveHub");

app.Run();
