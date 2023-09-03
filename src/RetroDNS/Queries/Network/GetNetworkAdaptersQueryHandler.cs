using System.Net.NetworkInformation;
using MediatR;

namespace RetroDNS.Queries.Network;

public class GetNetworkAdaptersQueryHandler : IRequestHandler<GetNetworkAdaptersQuery, NetworkInterface[]>
{
    public Task<NetworkInterface[]> Handle(GetNetworkAdaptersQuery request, CancellationToken cancellationToken)
    {
        return Task.FromResult(NetworkInterface.GetAllNetworkInterfaces()
            .Where(a =>
                a is { OperationalStatus: OperationalStatus.Up }
                    and ({NetworkInterfaceType: NetworkInterfaceType.Wireless80211} or {NetworkInterfaceType: NetworkInterfaceType.Ethernet})
                && a.Supports(NetworkInterfaceComponent.IPv4)
               )

            .ToArray());
    }
}
