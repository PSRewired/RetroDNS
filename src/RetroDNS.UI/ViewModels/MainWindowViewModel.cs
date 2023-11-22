using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using ReactiveUI;
using System.Reactive.Concurrency;
using MediatR;
using RetroDNS.Queries.Network;
using Serilog;
using Serilog.Core;
using Serilog.Events;

namespace RetroDNS.UI.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    private readonly IMediator _mediator;

    public ObservableCollection<NetworkInterface> Adapters { get; private set; } = new();
    public ObservableCollection<string> LogMessages { get; private set; } = new();

    private int _selectedAdapterIndex;

    public int SelectedAdapterIndex
    {
        get => _selectedAdapterIndex;
        set
        {
            this.RaiseAndSetIfChanged(ref _selectedAdapterIndex, value);
            this.RaisePropertyChanged(nameof(SelectedAdapter));
            this.RaisePropertyChanged(nameof(SelectedAdapterAddress));
        }
    }

    public void AddLogMessage(string message)
    {
        LogMessages.Add(message);
        this.RaisePropertyChanged(nameof(LogMessages));
    }

    private bool _serverRunning;

    public bool ServerRunning
    {
        get => _serverRunning;
        set => this.RaiseAndSetIfChanged(ref _serverRunning, value);
    }

    private string _serverStartButtonText = "Start";

    public string ServerStartButtonText
    {
        get => _serverStartButtonText;
        set => this.RaiseAndSetIfChanged(ref _serverStartButtonText, value);
    }

    public NetworkInterface SelectedAdapter => Adapters[SelectedAdapterIndex];

    public string SelectedAdapterAddress => SelectedAdapter.GetIPProperties().UnicastAddresses
        .First(a => a.Address.AddressFamily == AddressFamily.InterNetwork).Address.ToString();

    public bool HasAdapters => Adapters.Count > 0;
    public string[] LogLevels => Enum.GetNames(typeof(LogEventLevel));

    private int _loglevelIdx = (int)LogEventLevel.Information;

    public int LogLevelIdx
    {
        get => _loglevelIdx;
        set => this.RaiseAndSetIfChanged(ref _loglevelIdx, value);
    }

    public MainWindowViewModel(IMediator mediator, LoggingLevelSwitch levelSwitch)
    {
        _mediator = mediator;

        RxApp.MainThreadScheduler.Schedule(LoadNetworkAdapters);
        this.WhenAnyValue(p => p.LogLevelIdx)
            .Subscribe(v =>
            {
                levelSwitch.MinimumLevel = (LogEventLevel)v;
                Log.Warning("Log level was updated to {LogLevel}", levelSwitch.MinimumLevel);
            });
    }


    private async void LoadNetworkAdapters()
    {
        var adapters = await _mediator.Send(new GetNetworkAdaptersQuery());

        Adapters = new ObservableCollection<NetworkInterface>(adapters);
    }
}
