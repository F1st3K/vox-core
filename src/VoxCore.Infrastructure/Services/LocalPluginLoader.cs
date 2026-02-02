using System.Reflection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VoxCore.Plugins.Contracts;
using VoxCore.Runtime.Contracts;
using static VoxCore.Runtime.Contracts.IPluginLoader;


namespace VoxCore.Infrastructure.Services;

public sealed class LocalPluginLoader(
    ILogger<LocalPluginLoader> logger,
    IOptions<Settings.Options.Plugins> config
) : IPluginLoader
{
    public async IAsyncEnumerable<Plugin> LoadPlugins()
    {
        string path;
        if (Path.IsPathRooted(config.Value.PluginsPath))
        {
            path = config.Value.PluginsPath;
        }
        else
        {
            path = Path.Combine(AppContext.BaseDirectory, config.Value.PluginsPath);
            path = Path.GetFullPath(path);
        }

        if (!Directory.Exists(path))
        {
            logger.LogError($"Failed load plugins, not exist path {path}");
            yield break;
        }

        foreach (var dll in Directory.GetFiles(path, "*.dll"))
        {
            Assembly assembly;
            try
            {
                assembly = await Task.Run(() => Assembly.LoadFrom(dll));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Failed on load dll: {dll}, skipped...");
                continue;
            }

            foreach (var type in assembly.GetTypes())
            {
                if (!type.IsAbstract && typeof(IPlugin).IsAssignableFrom(type))
                {
                    var genericTypes = type.GetInterfaces()
                        .FirstOrDefault(i => 
                                i.IsGenericType 
                                && i.GetGenericTypeDefinition() 
                                == typeof(IPlugin<,>)
                            )
                        ?.GetGenericArguments();

                    var intentType = genericTypes?.FirstOrDefault();
                    if (intentType == null)
                    {
                        logger.LogWarning($"Failed on load plugin: {type.Name} - intent type not valid, skipped...");
                        continue;
                    }

                    var parametersType = genericTypes?.LastOrDefault();
                    if (parametersType == null)
                    {
                        logger.LogWarning($"Failed on load plugin: {type.Name} - parameters type not valid, skipped...");
                        continue;
                    }

                    var intentInstance = Activator.CreateInstance(intentType);
                    if (intentInstance == null || intentInstance is not IIntentDeclaration intentDeclaration)
                    {
                        logger.LogWarning($"Failed on load plugin: {type.Name} - fail on create intent instanse, skipped...");
                        continue;
                    }

                    logger.LogInformation($"Loaded plugin:  {type},\n    intent:     {intentType + " : \"" + intentDeclaration.Name}\",\n    parameters: {parametersType};"
                    );
                    await Task.Yield();
                    yield return new Plugin(type, parametersType, intentDeclaration);
                }
            }
        }
    }
}