namespace VoxCore.Infrastructure.Settings;

public static partial class Options
{
    public class Plugins : OptionsBase
    {
        public override string SectionName => nameof(Plugins);

        public string PluginsPath { get; init; } = null!;
    }
}