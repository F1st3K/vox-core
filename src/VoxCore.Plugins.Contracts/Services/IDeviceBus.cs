namespace VoxCore.Plugins.Contracts.Services;

public interface IDeviceBus
{
    public sealed record Message<T>(string Topic, T Payload);

    public Task PublishAsync<TData>(
        string topic,
        TData body
    );


    public Task<IAsyncDisposable> SubscribeAsync<TData>(
        string topic,
        Func<Message<TData>, Task> handler
    );
}
