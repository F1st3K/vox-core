using System.Dynamic;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using VoxCore.Infrastructure.Contracts;

namespace VoxCore.Infrastructure.Workers;

public sealed class RabbitBusWorker(
    IOptions<Settings.Options.RabbitMQ> options,
    ILogger<RabbitBusWorker> logger
) : BackgroundService, IRabbitBus
{
    private IChannel? _publishChannel;
    private IChannel? _consumeChannel;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            var cf = new ConnectionFactory
            {
                Uri = new Uri(options.Value.ConnectionString),
            };

            var connection = await cf.CreateConnectionAsync(cancellationToken: stoppingToken);
            _publishChannel = await connection.CreateChannelAsync(cancellationToken: stoppingToken);
            _consumeChannel = await connection.CreateChannelAsync(cancellationToken: stoppingToken);

            logger.LogInformation("RabbitMQ Bus started ...");

            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        catch (OperationCanceledException)
        {
            logger.LogInformation("Worker stopping due to cancellation");
        }
    }

    public async Task PublishAsync(
        string exchange,
        string routingKey,
        object body,
        CancellationToken ct = default)
    {
        if (_publishChannel is null) throw new InvalidOperationException("Bus not started");

        await _publishChannel.BasicPublishAsync(
            exchange: exchange,
            routingKey: routingKey,
            mandatory: false,
            body: Encoding.UTF8.GetBytes(JsonSerializer.Serialize(body)),
            cancellationToken: ct
        );
    }

    public async Task SubscribeAsync(
        string exchange,
        string routingKey,
        Func<string, dynamic, CancellationToken, Task> handler,
        CancellationToken ct = default)
    {
        if (_consumeChannel is null) throw new InvalidOperationException("Bus not started");

        var queueName = (await _consumeChannel.QueueDeclareAsync(
            queue: "",
            durable: false,
            exclusive: true,
            autoDelete: true,
            cancellationToken: ct
        )).QueueName;

        await _consumeChannel.QueueBindAsync(
            queue: queueName,
            exchange: exchange,
            routingKey: routingKey,
            cancellationToken: ct
        );

        var consumer = new AsyncEventingBasicConsumer(_consumeChannel);
        consumer.ReceivedAsync += async (_, ea) =>
        {
            try
            {
                dynamic? body = JsonSerializer.Deserialize<ExpandoObject>(
                    Encoding.UTF8.GetString(ea.Body.Span));

                await handler(ea.Exchange, body, ct);

                await _consumeChannel.BasicAckAsync(ea.DeliveryTag, false);
            }
            catch (OperationCanceledException)
            {
                logger.LogInformation("Message handling cancelled");
                await _consumeChannel.BasicNackAsync(ea.DeliveryTag, false, true);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Message handling failed");
                await _consumeChannel.BasicNackAsync(ea.DeliveryTag, false, true);
            }
        };

        await _consumeChannel.BasicConsumeAsync(
            queue: queueName,
            autoAck: false,
            consumer: consumer,
            cancellationToken: ct
        );
    }

    public async Task<dynamic> RpcAsync(
        string exchange,
        string routingKey,
        object body,
        CancellationToken ct = default)
    {
        if (_publishChannel is null || _consumeChannel is null)
            throw new InvalidOperationException("Bus not started");

        string correlationId = Guid.NewGuid().ToString();

        var replyQueue = (await _consumeChannel.QueueDeclareAsync(
            queue: "",
            durable: false,
            exclusive: true,
            autoDelete: true,
            cancellationToken: ct
        )).QueueName;

        var tcs = new TaskCompletionSource<dynamic?>();

        var consumer = new AsyncEventingBasicConsumer(_consumeChannel);
        consumer.ReceivedAsync += async (_, ea) =>
        {
            try
            {
                if (ea.BasicProperties?.CorrelationId == correlationId)
                {
                    dynamic? response = JsonSerializer.Deserialize<ExpandoObject>(
                        Encoding.UTF8.GetString(ea.Body.Span)
                    );

                    tcs.TrySetResult(response);
                    await _consumeChannel.BasicAckAsync(ea.DeliveryTag, false);
                }
                else
                {
                    await _consumeChannel.BasicNackAsync(ea.DeliveryTag, false, true);
                }
            }
            catch (Exception ex)
            {
                tcs.TrySetException(ex);
                await _consumeChannel.BasicNackAsync(ea.DeliveryTag, false, true);
            }
        };

        string consumerTag = await _consumeChannel.BasicConsumeAsync(
            queue: replyQueue,
            autoAck: false,
            consumer: consumer,
            cancellationToken: ct
        );

        byte[] bodyBytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(body));

        var props = new BasicProperties();
        props.CorrelationId = correlationId;
        props.ReplyTo = replyQueue;

        await _publishChannel.BasicPublishAsync(
            exchange: exchange,
            routingKey: routingKey,
            mandatory: false,
            basicProperties: props,
            body: bodyBytes,
            cancellationToken: ct
        );

        using (ct.Register(() => tcs.TrySetCanceled()))
        {
            return await tcs.Task ?? new {};
        }
    }
}
