using Microsoft.Extensions.Logging;
using VoxCore.Runtime.Contracts;
using VoxCore.Runtime.Services;

namespace VoxCore.Runtime;

public class VoxCoreRuntime(
    PluginExecutor executor,
    ParameterRefiner refiner,
    IPluginLoader loader,
    IInputService input,
    IIntentService nlu,
    IConversationService conversation,
    ILogger<VoxCoreRuntime> logger
) : IRuntimeEntry
{
    private IPluginLoader.Plugin[] _plugins = [];

    private CancellationToken _runtimeStopper;

    public async Task RunAsync(CancellationToken ct)
    {
        _runtimeStopper = ct;

        using var loaderCTS = NewStopper(TimeSpan.FromMinutes(1));
        _plugins = await loader.LoadPlugins().ToArrayAsync(loaderCTS.Token);

        using var nluConfCTS = NewStopper(TimeSpan.FromMinutes(3));
        await nlu.ConfigureAsync(
            _plugins.Select(p => p.Declaration),
            nluConfCTS.Token
        );

        input.InputReceived += InputHandler;

        ct.Register(Stop);
    }

    private void Stop()
    {
        input.InputReceived -= InputHandler;

        _plugins = [];
        _runtimeStopper = CancellationToken.None;
    }

    private async void InputHandler(object? sender, IInputService.Args e)
    {
        using var executerCTS = NewStopper();
        var dialog = new CurrentDialog(conversation, Guid.NewGuid(), e.Source, executerCTS.Token);
        if (e.Message == null)
        {
            dialog.Say("Неслышу");
            return;
        }

        try
        {
            using var nluCTS = NewStopper(TimeSpan.FromSeconds(15));
            var intent = await nlu.DecodeAsync(e.Message, nluCTS.Token);
            if (intent == null)
            {
                dialog.Say("Непонимаю");
                return;
            }
            
            var p = _plugins.FirstOrDefault(p => p.Declaration.Name == intent.Name);
            if (p == null)
            {
                dialog.Say("Я такое не умею");
                return;
            }

            var isRefined = await refiner.TryRefining(p.ParametersType, intent.Parameters, dialog);
            if (!isRefined)
            {
                dialog.Say("Ой... не хватило параметров...");
                return;
            }

            var isExecuted = await executor.TryExecuteAsync(p.PluginType, intent.Parameters, dialog, executerCTS.Token);
            if (!isExecuted)
            {
                dialog.Say("Ой... что-то пошло не так..");
                return;
            }
        }
        catch (OperationCanceledException) when (_runtimeStopper.IsCancellationRequested)
        {
            dialog.Say("Завершение работы");
            logger.LogInformation("Input process is stopped...");
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex, "Unhandled error on input process...");
        }
    }

    private CancellationTokenSource NewStopper(TimeSpan? timeout = null)
    {
        var cts = CancellationTokenSource.CreateLinkedTokenSource(_runtimeStopper);
        if (timeout.HasValue)
            cts.CancelAfter(timeout.Value);
        return cts;
    }
}