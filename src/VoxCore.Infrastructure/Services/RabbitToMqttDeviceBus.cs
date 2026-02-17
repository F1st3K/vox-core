using Microsoft.Extensions.Options;
using VoxCore.Infrastructure.Contracts;
using VoxCore.Plugins.Contracts.Services;

namespace VoxCore.Infrastructure.Services;

public sealed class RabbitToMqttDeviceBus(
    IOptions<Settings.Options.RabbitMQ> options,
    IRabbitBus rabbitBus
) : IDeviceBus
{
    public async Task PublishAsync<TData>(string topic, TData body)
    {
        await rabbitBus.PublishAsync(
            exchange: options.Value.MqttExchange,
            routingKey: topic.Replace('/', '.'),
            body: body
        );
    }

    public async Task<IAsyncDisposable> SubscribeAsync<TData>(string topic, Func<IDeviceBus.Message<TData>, Task> handler)
    {
        return await rabbitBus.SubscribeAsync<TData>(
            exchange: options.Value.MqttExchange,
            routingKey: topic.Replace('/', '.'),
            async (routingKey, payload, ct) =>
            {
                var message = new IDeviceBus.Message<TData>(routingKey, payload);
                await handler(message);
            });
        }
}