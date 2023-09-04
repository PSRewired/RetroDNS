using System;
using Avalonia.Controls;
using Microsoft.Extensions.DependencyInjection;

namespace RetroDNS.UI.DependencyInjection;

public static class DependencyInjectionExtensions
{
    public static IServiceProvider GetServiceProvider(this IResourceHost control) {
        return (IServiceProvider) control.FindResource(typeof(IServiceProvider))!;
    }

    public static T CreateInstance<T>(this IResourceHost control) {
        return ActivatorUtilities.CreateInstance<T>(control.GetServiceProvider());
    }
}
