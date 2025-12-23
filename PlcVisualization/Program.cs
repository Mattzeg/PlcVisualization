using PlcVisualization.Components;
using PlcVisualization.Services;
using PlcVisualization.Hubs;
using PlcVisualization.Models;
using PlcVisualization.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// SignalR für Real-time Updates
builder.Services.AddSignalR();

// Database Context mit SQLite
builder.Services.AddDbContextFactory<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")
        ?? "Data Source=plcvisualization.db"));

// Configuration Service (Singleton, da er von PlcService verwendet wird)
builder.Services.AddSingleton<ConfigurationService>();

// Drive Logging Service (Singleton)
builder.Services.AddSingleton<DriveLoggingService>();

// PLC Settings aus appsettings.json laden
builder.Services.Configure<PlcSettings>(builder.Configuration.GetSection("PlcSettings"));

// PLC Service als Singleton und HostedService
builder.Services.AddSingleton<PlcService>();
builder.Services.AddHostedService(sp => sp.GetRequiredService<PlcService>());

var app = builder.Build();

// Datenbank initialisieren
using (var scope = app.Services.CreateScope())
{
    var dbContextFactory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<ApplicationDbContext>>();
    using var context = await dbContextFactory.CreateDbContextAsync();
    await context.Database.EnsureCreatedAsync();

    // LoggingService mit ConfigurationService verbinden (zirkuläre Abhängigkeit auflösen)
    var configService = scope.ServiceProvider.GetRequiredService<ConfigurationService>();
    var loggingService = scope.ServiceProvider.GetRequiredService<DriveLoggingService>();
    configService.SetLoggingService(loggingService);
}

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
