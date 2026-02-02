using Microsoft.Extensions.DependencyInjection;
using VoxCore.Runtime.Contracts;
using VoxCore.Runtime.Services;

namespace VoxCore.Runtime;

public static class DependencyInjection
{
    public static IServiceCollection AddRuntimeEntry(this IServiceCollection services)
    {
        services.AddSingleton<PluginExecutor>();
        services.AddSingleton<ParameterBuilder>();
        services.AddSingleton<ParameterRefiner>();

        services.AddSingleton<IRuntimeEntry, VoxCoreRuntime>();

        return services;
    }
}