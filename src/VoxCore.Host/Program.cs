using VoxCore.Infrastructure;
using VoxCore.Runtime;

var builder = Host.CreateApplicationBuilder(args);

builder.Services
    .AddLogging()
    .AddRuntimeEntry()
    .AddInfrastructure(builder.Configuration);

var host = builder.Build();
host.Run();
