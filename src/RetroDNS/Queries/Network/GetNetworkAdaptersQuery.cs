using System.Net.NetworkInformation;
using MediatR;

namespace RetroDNS.Queries.Network;

/// <summary>
/// Returns a list of network adapters that are online and support IPv4 + DNS
/// </summary>
public class GetNetworkAdaptersQuery : IRequest<NetworkInterface[]>
{
}
