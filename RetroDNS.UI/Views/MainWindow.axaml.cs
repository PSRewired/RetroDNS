using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using Microsoft.Extensions.DependencyInjection;
using ReactiveUI;
using RetroDNS.UI.DependencyInjection;
using RetroDNS.UI.Logging;
using RetroDNS.UI.Services;
using RetroDNS.UI.ViewModels;
using Serilog;

namespace RetroDNS.UI.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        AvaloniaXamlLoader.Load(this);
#if DEBUG
        this.AttachDevTools();
#endif

        this.GetServiceProvider().GetRequiredService<AvaloniaSink>().Subscriber =
            message => Dispatcher.UIThread.Invoke(() => (DataContext as MainWindowViewModel)?.AddLogMessage(message));
    }

    private async void OnServerStartClicked(object? sender, RoutedEventArgs e)
    {
        var model = (DataContext as MainWindowViewModel)!;
        var serverService = this.GetServiceProvider().GetRequiredService<ServerBackgroundService>();

        if (serverService.IsActive)
        {
            serverService.Stop();
        }

        if (serverService.IsActive)
        {
            serverService.Stop();
        }
        else
        {
            serverService.Start(model.SelectedAdapterAddress);
        }

        model.ServerRunning = serverService.IsActive;
        model.ServerStartButtonText = serverService.IsActive ? "Stop" : "Start";
    }

    private void OnClearLogsClicked(object? sender, RoutedEventArgs e)
    {
        (DataContext as MainWindowViewModel)!.LogMessages.Clear();
        Log.Warning("Log was cleared");
    }
}

