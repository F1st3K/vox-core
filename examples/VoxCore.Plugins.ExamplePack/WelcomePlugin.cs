using VoxCore.Plugins.Contracts;
using VoxCore.Plugins.Contracts.Services;

namespace VoxCore.Plugins.ExamplePack;

public class WelcomePlugin() : PluginBase<WelcomePlugin, NoParameters>, IIntentDeclaration
{
    public override WelcomePlugin Intent => this;

    public string Name => "welcome";

    public IEnumerable<string> Examples =>
    [
        "Привет",
        "Ты кто?",
        "Как ты?"
    ];

    private readonly ICurrentDialog _dialog = null!;

    public WelcomePlugin(ICurrentDialog dialog) : this()
    {
        _dialog = dialog;
    }

    public override async Task ExecuteAsync(NoParameters parameters, CancellationToken ct)
    {
        var name = await _dialog.AskAsync("Привет, я твой помощник, а как зовут тебя?");
        _dialog.Say($"{name}, прекрасное имя!");
    }
}