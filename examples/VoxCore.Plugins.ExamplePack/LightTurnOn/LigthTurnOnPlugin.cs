using VoxCore.Plugins.Contracts;
using VoxCore.Plugins.Contracts.Services;

namespace VoxCore.Plugins.ExamplePack.LightTurnOn;

public class LightTurnOnPlugin(
    ICurrentDialog dialog,
    IDeviceBus devices
) : PluginBase<Intent, Params>
{
    public override Intent Intent { get; } = new Intent();

    public override async Task ExecuteAsync(Params parameters, CancellationToken ct)
    {
        await devices.PublishAsync("home/device1/set", "ON");
        await devices.PublishAsync("home/device1/set", new { state = "ON" });
        dialog.Say($"Включаю свет в {parameters.Room}");
    }
}