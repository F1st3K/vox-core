namespace VoxCore.Infrastructure.Contracts;

public interface IRabbitBus
{
    public Task PublishAsync<TData>(
        string exchange,
        string routingKey,
        TData body,
        CancellationToken ct = default
    );


    public Task<IAsyncDisposable> SubscribeAsync<TData>(
        string exchange,
        string routingKey,
        Func<string, TData, CancellationToken, Task> handler,
        CancellationToken ct = default
    );
}