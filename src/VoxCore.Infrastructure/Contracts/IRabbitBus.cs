namespace VoxCore.Infrastructure.Contracts;

public interface IRabbitBus
{
    public Task PublishAsync(
        string exchange,
        string routingKey,
        object body,
        CancellationToken ct = default
    );


    public Task SubscribeAsync(
        string exchange,
        string routingKey,
        Func<string, dynamic, CancellationToken, Task> handler,
        CancellationToken ct = default
    );

    public Task<dynamic> RpcAsync(
        string exchange,
        string routingKey,
        object body,  
        CancellationToken ct = default
    );
}