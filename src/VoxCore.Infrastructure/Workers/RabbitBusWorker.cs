using System.Dynamic;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using VoxCore.Infrastructure.Contracts;
using VoxCore.Infrastructure.Services;

namespace VoxCore.Infrastructure.Workers;

public sealed class RabbitBusWorker(
    IOptions<Settings.Options.RabbitMQ> options,
    ILogger<RabbitBusWorker> logger
) : BackgroundService, IRabbitBus
{
    private readonly TaskCompletionSource<IChannel> _publishChannelTcs = new(TaskCreationOptions.RunContinuationsAsynchronously);
    private readonly TaskCompletionSource<IConnection> _connectionTcs = new(TaskCreationOptions.RunContinuationsAsynchronously);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            var cf = new ConnectionFactory { Uri = new Uri(options.Value.ConnectionString) };

            var connection = await cf.CreateConnectionAsync(cancellationToken: stoppingToken);
            _connectionTcs.SetResult(connection);

            _publishChannelTcs.SetResult(
                await connection.CreateChannelAsync(cancellationToken: stoppingToken));

            logger.LogInformation("RabbitMQ Bus started ...");

            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        catch (OperationCanceledException)
        {
            logger.LogInformation("Worker stopping due to cancellation");
        }
    }

    public async Task PublishAsync<TData>(
        string exchange,
        string routingKey,
        TData body,
        CancellationToken ct = default)
    {
        var publishChannel = await _publishChannelTcs.Task;

        await publishChannel.ExchangeDeclareAsync(
            exchange,
            type: "topic",
            durable: true,
            autoDelete: false,
            cancellationToken: ct
        );

        await publishChannel.BasicPublishAsync(
            exchange: exchange,
            routingKey: routingKey,
            mandatory: false,
            body: Encoding.UTF8.GetBytes(JsonSerializer.Serialize(body)),
            cancellationToken: ct
        );
    }

    public async Task<IAsyncDisposable> SubscribeAsync<TData>(
        string exchange,
        string routingKey,
        Func<string, TData, CancellationToken, Task> handler,
        CancellationToken ct = default)
    {
        var connection = await _connectionTcs.Task;
        var channel = await connection.CreateChannelAsync(cancellationToken: ct);

        await channel.BasicQosAsync(0, 1, false, ct);

        await channel.ExchangeDeclareAsync(
            exchange,
            type: "topic",
            durable: true,
            autoDelete: false,
            cancellationToken: ct
        );

        var queueName = (await channel.QueueDeclareAsync(
            queue: "",
            durable: false,
            exclusive: true,
            autoDelete: true,
            cancellationToken: ct
        )).QueueName;

        await channel.QueueBindAsync(queueName, exchange, routingKey, cancellationToken: ct);

        var consumer = new AsyncEventingBasicConsumer(channel);

        consumer.ReceivedAsync += async (_, ea) =>
        {
            try
            {
                var data = Encoding.UTF8.GetString(ea.Body.Span);
                var body = JsonSerializer.Deserialize<TData>(data, JsonSerializerOptions.Web)!;

                await handler(ea.Exchange, body, ct);
                await channel.BasicAckAsync(ea.DeliveryTag, false);
            }
            catch (OperationCanceledException)
            {
                logger.LogDebug("Message handling cancelled");
                await channel.BasicNackAsync(ea.DeliveryTag, false, true);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Message handling failed");
                await channel.BasicNackAsync(ea.DeliveryTag, false, true);
            }
        };

        var consumerTag = await channel.BasicConsumeAsync(
            queue: queueName,
            autoAck: false,
            consumer: consumer,
            cancellationToken: ct
        );

        return new RabbitSubscription(channel, consumerTag, logger);
    }

}
