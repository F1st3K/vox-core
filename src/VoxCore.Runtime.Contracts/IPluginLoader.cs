using VoxCore.Plugins.Contracts;

namespace VoxCore.Runtime.Contracts;

public interface IPluginLoader
{
    public sealed record Plugin(
        Type PluginType,
        Type ParametersType,
        IIntentDeclaration Declaration
    );

    public IAsyncEnumerable<Plugin> LoadPlugins();
}