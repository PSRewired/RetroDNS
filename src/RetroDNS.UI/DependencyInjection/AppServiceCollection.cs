using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using RetroDNS.Forwarders;
using RetroDNS.Resolvers;
using RetroDNS.UI.Logging;
using RetroDNS.UI.Services;
using RetroDNS.UI.ViewModels;
using Serilog;
using Serilog.Core;

namespace RetroDNS.UI.DependencyInjection;

public class AppServiceCollection
{
    public IServiceProvider Build()
    {
        var services = new ServiceCollection();

        var viewModels = GetType().Assembly.DefinedTypes.Where(t => t.IsAssignableTo(typeof(ViewModelBase)) && !t.IsAbstract)
            .ToArray();

        foreach (var model in viewModels)
        {
            services.AddSingleton(model);
        }

        var avaloniaSink = new AvaloniaSink();
        services.AddSingleton(avaloniaSink);

        var logLevelSwitch = new LoggingLevelSwitch();
        services.AddSingleton(logLevelSwitch);

        var logger = new LoggerConfiguration()
            .MinimumLevel.ControlledBy(logLevelSwitch)
            .WriteTo.Debug()
            .WriteTo.Sink(avaloniaSink)
            .CreateLogger();

        Log.Logger = logger;

        services.AddLogging(builder => builder.AddSerilog(logger, dispose: true));
        services.AddSingleton<DomainCacheResolver>();
        services.AddScoped<DnsPacketForwarder>();
        services.AddSingleton<ServerBackgroundService>();

        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssemblyContaining<Program>();
            cfg.RegisterServicesFromAssemblyContaining<EntryPoint>();
        });

        return services.BuildServiceProvider();
    }
}
