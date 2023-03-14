using System;
using System.Collections.ObjectModel;
using System.Net.NetworkInformation;
using ReactiveUI;
using System.Reactive.Concurrency;
using MediatR;
using RetroDNS.Queries.Network;

namespace RetroDNS.UI.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    private readonly IMediator _mediator;

    public ObservableCollection<NetworkInterface> Adapters { get; private set; } = new();
    public int SelectedAdapter { get; set; } = 0;

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
