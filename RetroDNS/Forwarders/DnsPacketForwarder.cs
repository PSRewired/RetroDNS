using System.Net;
using System.Net.Sockets;
using Serilog;

namespace RetroDNS.Forwarders;

public class DnsPacketForwarder
{
    /// <summary>
    /// Forwards and waits for a UDP response from the given DNS server endpoint
    /// </summary>
    /// <param name="dnsEndpoint">The connection information for the DNS server to forward to</param>
    /// <param name="dnsQuestion"></param>
    /// <returns>The query response from the DNS server in bytes</returns>
    /// <exception cref="TimeoutException">When no reply was received from the DNS server in 5 seconds</exception>
    public async Task<byte[]> SendDnsQuery(IPEndPoint dnsEndpoint, byte[] dnsQuestion)
    {
        Log.Debug("Forwarding DNS query to {Endpoint}", dnsEndpoint.ToString());
        
        var client = new UdpClient(dnsEndpoint.Address.ToString(), dnsEndpoint.Port);

        await client.SendAsync(dnsQuestion);

        var result = await client.ReceiveAsync()
            .WaitAsync(TimeSpan.FromSeconds(5)); // This timeout is probably too long

        return result.Buffer;
    }
}