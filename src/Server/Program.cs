global using Shared.Data;
global using Server.Hubs;
global using Shared.Entity;
global using Shared.Models;
global using Shared.Helpers;
global using Shared.Services;
global using Shared.Interfaces;
using Server.Services;
// using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.AspNetCore.Components.Server.Circuits;

// startup logger
using var loggerFactory = LoggerFactory.Create(loggingBuilder => loggingBuilder
    .SetMinimumLevel(LogLevel.Trace)
    .AddConsole());
ILogger logger = loggerFactory.CreateLogger<Program>();


var builder = WebApplication.CreateBuilder(args);
builder.Logging.ClearProviders().AddConsole();
// .AddFileLogger();

// api
builder.Services.AddControllers();
builder.Services.AddRouting(opt => opt.LowercaseUrls = true);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApiDocument(options =>
    options.Title = "timeNet aAPI"
);
builder.Services.AddHealthChecks();
builder.Services.AddRazorPages(); // errorpg

// db
builder.Services.AddDbContextFactory<TimeNetDbContext>();
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// service 
builder.Services.AddScoped<ICommandService, CommandService>();
builder.Services.AddScoped<IStatsService, StatsService>();
builder.Services.AddScoped<IQueryService, QueryService>();
// builder.Services.AddScoped<IBackupService, BackupService>();

// hub
/* builder.Services.AddResponseCompression(options =>{
    options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(new[] { "application/octet-stream" });
}); */
builder.Services.AddSignalR();
builder.Services.AddSingleton<CircuitHandler, TimeHubCircuitHandler>();


var app = builder.Build();

// init db, css
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<TimeNetDbContext>();
    db.Database.EnsureCreated();
}

// dev env 
if (app.Environment.IsDevelopment())
{
    logger.LogInformation("Running in development environment.");
    app.UseDeveloperExceptionPage();
    app.UseWebAssemblyDebugging();
}
else 
{
    logger.LogInformation("Running in production environment.");
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

// mapping
/* cause some errors using signalr hub with self-signed, linux */ //app.UseHttpsRedirection();
app.UseRouting();

// api
app.MapControllers();
app.MapHealthChecks("/health");
app.MapRazorPages(); // error page

app.UseOpenApi();
app.UseSwaggerUi3();
app.UseReDoc(config => config.Path = "/redoc");

// hub
app.MapHub<EventsService>("/signal");
/* app.UseResponseCompression(); */

// webApp
app.UseBlazorFrameworkFiles();
app.MapFallbackToFile("index.html");

app.UseStaticFiles();

app.Run();

