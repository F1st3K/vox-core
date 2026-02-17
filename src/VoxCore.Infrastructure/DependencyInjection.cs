using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using VoxCore.Runtime.Contracts;
using VoxCore.Infrastructure.Services;
using VoxCore.Infrastructure.Workers;
using VoxCore.Infrastructure.Contracts;
using VoxCore.Plugins.Contracts.Services;

namespace VoxCore.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfigurationManager config)
    {
        services
            .AddSettings(config)
            .AddServices()
            .AddWorkers();

        return services;
    }

    private static IServiceCollection AddSettings(this IServiceCollection services, IConfigurationManager config)
    {
        services.AddSingleton(config.GetOptions<Settings.Options.Plugins>());
        services.AddSingleton(config.GetOptions<Settings.Options.RabbitMQ>());

        return services;
    }

    private static IOptions<TOptions> GetOptions<TOptions>(this IConfigurationManager config)
        where TOptions : Settings.OptionsBase, new()
    {
        var settings = new TOptions();
        config.Bind(settings.SectionName, settings);
        return Options.Create(settings);
    }

    private static IServiceCollection AddServices(this IServiceCollection services)
    {
        services.AddSingleton<RabbitBusWorker>();
        services.AddSingleton<IRabbitBus>(sp => sp.GetRequiredService<RabbitBusWorker>());

        services.AddSingleton<IPluginLoader, LocalPluginLoader>();
        services.AddSingleton<IInputService, RabbitInputService>();
        services.AddSingleton<IIntentService, RabbitIntentService>();
        services.AddSingleton<IConversationService, RabbitConversationService>();
        services.AddSingleton<IDeviceBus, RabbitToMqttDeviceBus>();

        services.AddTransient<ITextNormalaizer, Services.Normalizers.Number>();
        // services.AddTransient<ITextNormalaizer, Services.Normalizers.Date>();
        // services.AddTransient<ITextNormalaizer, Services.Normalizers.Weekday>();
        // services.AddTransient<ITextNormalaizer, Services.Normalizers.Time>();
        // services.AddTransient<ITextNormalaizer, Services.Normalizers.Period>();

        return services;
    }

    private static IServiceCollection AddWorkers(this IServiceCollection services)
    {
        services.AddHostedService<RuntimeWorker>();
        services.AddHostedService(sp => sp.GetRequiredService<RabbitBusWorker>());

        return services;
    }
}