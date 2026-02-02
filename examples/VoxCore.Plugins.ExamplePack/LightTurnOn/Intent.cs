using VoxCore.Plugins.Contracts;

namespace VoxCore.Plugins.ExamplePack.LightTurnOn;

public class Intent() : IIntentDeclaration
{
    public string Name => "light-turn-on";

    public IEnumerable<string> Examples =>
    [
        $"Включи свет на [кухне]({nameof(Params.Room)})",
        $"Включи свет в [аквариуме]({nameof(Params.Room)})",
        $"Включи свет в [комнате]({nameof(Params.Room)})"
    ];
}