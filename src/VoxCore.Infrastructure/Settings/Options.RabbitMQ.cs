namespace VoxCore.Infrastructure.Settings;

public static partial class Options
{
    public class RabbitMQ : OptionsBase
    {
        public override string SectionName => nameof(RabbitMQ);

        public string ConnectionString { get; init; } = null!;

        public string MqttExchange { get; init; } = null!;
    }
}