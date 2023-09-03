using System.Collections.ObjectModel;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using ReactiveUI;
using System.Reactive.Concurrency;
using MediatR;
using RetroDNS.Queries.Network;

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

    public MainWindowViewModel(IMediator mediator)
    {
        _mediator = mediator;

        RxApp.MainThreadScheduler.Schedule(LoadNetworkAdapters);
    }


    private async void LoadNetworkAdapters()
    {
        var adapters = await _mediator.Send(new GetNetworkAdaptersQuery());

        Adapters = new ObservableCollection<NetworkInterface>(adapters);
    }
}
