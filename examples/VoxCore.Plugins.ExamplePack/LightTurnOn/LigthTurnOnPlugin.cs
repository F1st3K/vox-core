using VoxCore.Plugins.Contracts;
using VoxCore.Plugins.Contracts.Services;

namespace VoxCore.Plugins.ExamplePack.LightTurnOn;

public class LightTurnOnPlugin(
    ICurrentDialog dialog
) : PluginBase<Intent, Params>
{
    public override Intent Intent { get; } = new Intent();

    public override async Task ExecuteAsync(Params parameters, CancellationToken ct)
    {
        dialog.Say($"Включаю свет в {parameters.Room}");
    }
}