global using Shared.Models;
global using Shared.Entity;
global using Shared.Helpers;
global using Shared.Services;
global using Shared.Interfaces;
global using Shared.Client;
using Web;
using Web.Services;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.AspNetCore.Components.Web;


var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// api
builder.Services.AddHttpClient<IIntervalsClient, IntervalsClient>(
    cl => cl.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress + "api")
);
builder.Services.AddHttpClient<ITagsClient, TagsClient>(
    cl => cl.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress + "api")
);
builder.Services.AddHttpClient<IConfigClient, ConfigClient>(
    cl => cl.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress + "api")
);

builder.Services.AddHttpClient<IStatisticsClient, StatisticsClient>(
    cl => cl.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress + "api")
);
builder.Services.AddHttpClient<IDeveloperClient, DeveloperClient>(
    cl => cl.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress + "api")
);

builder.Services.AddHttpClient();

// services
// builder.Services.AddScoped<IQueryService, QueryService>();
// builder.Services.AddScoped<ICommandService, CommandService>();

builder.Services.AddScoped<IQueryService, QueryServiceNSwag>();
builder.Services.AddScoped<ICommandService, CommandServiceNSwag>();

builder.Services.AddScoped<IStatsService, StatsService>();

builder.Services.AddScoped<IEventsServiceClient, EventsServiceClient>();
builder.Services.AddScoped<IEventsService, EventsServiceLocal>();


builder.Services.AddSingleton<StatusService>();


await builder.Build().RunAsync();


