using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using VoxCore.Plugins.Contracts;

namespace VoxCore.Runtime.Services;

public sealed class PluginExecutor(
    IServiceProvider services,
    ILogger<PluginExecutor> logger
)
{
    internal async Task<bool> TryExecuteAsync(
        Type pluginType,
        object parameters,
        CurrentDialog dialog,
        CancellationToken ct)
    {
        using var logScope = logger.BeginScope(new
        {
            dialog.SessionId,
            Plugin = pluginType.FullName,
            Parameters = parameters.GetType().Name
        });

        using var scope = services.CreateScope();
        var scopedServices = scope.ServiceProvider;

        var plugin = (IPlugin)ActivatorUtilities.CreateInstance(
            scopedServices,
            pluginType,
            dialog
        );

        try
        {
            await plugin.ExecuteAsync(parameters, ct);

            return true;
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex, "Failed execute plugin");

            return false;
        }
    }
}
