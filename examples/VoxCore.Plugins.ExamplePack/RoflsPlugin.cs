using VoxCore.Plugins.Contracts;
using VoxCore.Plugins.Contracts.Services;

namespace VoxCore.Plugins.ExamplePack;

public class RoflsPlugin(
    ICurrentDialog dialog
) : PluginBase<RoflsPlugin.IntentDeclaration, NoParameters>
{
    public class IntentDeclaration() : IIntentDeclaration
    {
        public string Name => "rofls";

        public IEnumerable<string> Examples =>
        [
            "Ты только имитация жизни",
            "Робот сочинит симфонию?",
            "Робот превратит кусок холтса в шедевр искуства",
            "Ты только имитация жизни, робот сочинит симфонию, робот превратит кусок холтса в шедевр искуства",
            "Ты бесполезен",
            "Ты тупой",
        ];
    }

    public override IntentDeclaration Intent { get; } = new IntentDeclaration();

    public override async Task ExecuteAsync(NoParameters parameters, CancellationToken ct)
    {
        dialog.Say($"А негр блять может мне тут не сидеть  блять, и рэп нахуй не исполнять нахуй? Слышишь ты нахуй, тумба-юмба ебаная, ты нахуй сьебись блять в свой эквадор и там нахуй, сиди блять, бананы нахуй жуй блять, понял нахуй? Дич нахуй, ебаная блять! Ты мне какого тут нахуй, слышь предьявляешь нахуй, может, не может? Ты то нахуй дохуя че смог? Нахуй, дич бля!?");
    }
}