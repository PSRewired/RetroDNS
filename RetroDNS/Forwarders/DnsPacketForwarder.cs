using System.Net;
using System.Net.Sockets;
using RetroDNS.Attributes;
using RetroDNS.Extensions;
using RetroDNS.Packets;
using RetroDNS.Resolvers;
using Serilog;

namespace RetroDNS.Forwarders;

public class DnsPacketForwarder
{
    public async Task<byte[]> AnswerQuery(ResolverEntry resolverEntry, DnsHeader header, Question dnsQuestion)
    {
        return resolverEntry.Type switch
        {
            ResolverEntry.ResolutionType.Ip => await CreateAnswer(resolverEntry, header, dnsQuestion),
            ResolverEntry.ResolutionType.Dns => await SendDnsQuery(resolverEntry, header, dnsQuestion),
            _ => throw new ArgumentOutOfRangeException($"Invalid entry type {resolverEntry.Type}")
        };
    }

    public Task<byte[]> CreateAnswer(ResolverEntry resolverEntry, DnsHeader header, Question dnsQuestion)
    {
        Log.Debug("Creating DNS response for A record resolution");
        var response = new DnsResponse
        {
            Header = header,
            Question = dnsQuestion,
            Answer = new Answer
            {
                Name = dnsQuestion.Domain,
                Domain = resolverEntry.Host,
            }
        };

        return Task.FromResult(response.Build());
    }

    /// <summary>
    /// Forwards and waits for a UDP response from the given DNS server endpoint
    /// </summary>
    /// <param name="dnsEndpoint">The connection information for the DNS server to forward to</param>
    /// <param name="dnsQuestion"></param>
    /// <returns>The query response from the DNS server in bytes</returns>
    /// <exception cref="TimeoutException">When no reply was received from the DNS server in 5 seconds</exception>
    public async Task<byte[]> SendDnsQuery(ResolverEntry resolverEntry, DnsHeader header, Question dnsQuestion)
    {
        Log.Debug("Forwarding DNS query to {Endpoint}", resolverEntry);

        using var stream = new MemoryStream();
        stream.Write(header.Serialize());
        stream.Write(dnsQuestion.Build());
        var client = new UdpClient(resolverEntry.Host.ToString(), 53);

        await client.SendAsync(stream.ToArray());

        var result = await client.ReceiveAsync()
            .WaitAsync(TimeSpan.FromSeconds(5)); // This timeout is probably too long

        return result.Buffer;
    }

    public async Task<byte[]> SendDnsQuery(ResolverEntry resolverEntry, byte[] stuff)
    {
        Log.Debug("Forwarding DNS query to {Endpoint}", resolverEntry);

        var client = new UdpClient(resolverEntry.Host.ToString(), 53);

        await client.SendAsync(stuff);

        var result = await client.ReceiveAsync()
            .WaitAsync(TimeSpan.FromSeconds(5)); // This timeout is probably too long

        return result.Buffer;
    }
}
