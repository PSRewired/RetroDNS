using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using RetroDNS.UI.ViewModels;

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

        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssemblyContaining<Program>();
            cfg.RegisterServicesFromAssemblyContaining<EntryPoint>();
        });

        return services.BuildServiceProvider();
    }
}
