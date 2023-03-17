using System.Data;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using Microsoft.Extensions.Options;
using RetroDNS.Attributes;
using RetroDNS.Extensions;
using RetroDNS.Forwarders;
using RetroDNS.Packets;
using RetroDNS.Resolvers;
using Serilog;

namespace RetroDNS.Server;

public class DnsServer
{
    private readonly DnsPacketForwarder _packetForwarder;
    private readonly DomainCacheResolver _domainResolver;
    private IPEndPoint EndPoint { get; }
    private Memory<byte> _receiveBuffer;
    private CancellationTokenSource? _tokenSource;
    private Task _listenTask;
    private Socket _udpListener;
    private readonly EndPoint _receiveFromEndpoint = new IPEndPoint(IPAddress.Any, 0);

    private ILogger Log => Serilog.Log.ForContext(GetType());

    private DnsServer(string address, int port) : this(IPAddress.Parse(address), port)
    {
    }

    private DnsServer(IPAddress address, int port)
    {
        EndPoint = new IPEndPoint(address, port);
        _receiveBuffer = GC.AllocateUninitializedArray<byte>(1024, true);
    }

    public DnsServer(IOptions<DnsServerConfiguration> configuration, DnsPacketForwarder packetForwarder, DomainCacheResolver domainResolver) : this(
        configuration.Value.BindIp, configuration.Value.Port)
    {
        _packetForwarder = packetForwarder;
        _domainResolver = domainResolver;
    }

    public void Start()
    {
        if (_tokenSource != null)
        {
            return;
        }

        _tokenSource = new CancellationTokenSource();

        _listenTask = Task.Run(async () => await Listen());
    }

    public void Stop()
    {
        Log.Information("Stopping RetroDNS...");
        _tokenSource?.Cancel();
    }

    private async Task Listen()
    {
        _udpListener = new Socket(SocketType.Dgram, ProtocolType.Udp);
        try
        {
            _udpListener.Bind(EndPoint);
            _udpListener.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.SendBuffer, 65535);
            _udpListener.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveBuffer, 65535);
        }
        catch (Exception e)
        {
            Log.Fatal(e, "[{ServerName}] Error while starting listener", GetType().Name);
        }

        Log.Information("RetroDNS is ready for use. Please enter your game DNS IP as {IPAddress}", EndPoint.Address.ToString());
        while (!_tokenSource?.IsCancellationRequested ?? false)
        {
            try
            {
                var result = await _udpListener.ReceiveFromAsync(_receiveBuffer, SocketFlags.None, _receiveFromEndpoint,
                    _tokenSource!.Token).ConfigureAwait(false);

                // Handle case where empty UDP packet(s) would throw an exception by trying to span an array by 0
                if (result.ReceivedBytes <= 1)
                {
                    continue;
                }

                await HandleReceive(_receiveBuffer[..result.ReceivedBytes], result.RemoteEndPoint);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (ObjectDisposedException)
            {
                break;
            }
            catch (ThreadInterruptedException)
            {
                break;
            }
            catch (Exception e)
            {
                Log.ForContext<DnsServer>().Error(e, "Error in listen loop");
            }
        }

        Log.Information("[{ClsName}] UDP receive thread stopped", GetType().Name);
        _udpListener.Dispose();
    }

    private async Task HandleReceive(Memory<byte> buffer, EndPoint ep)
    {
        try
        {
            Log.Debug("[{Class}] DNS packet received from ({IpAddress}:{Port})", GetType().Name,
                ((IPEndPoint)ep).Address.MapToIPv4(), ((IPEndPoint)ep).Port);
            Log.Verbose("{PacketData}", buffer.ToHexDump());

            var headerBytes = Marshal.SizeOf<DnsHeader>();
            if (buffer.Length < headerBytes)
            {
                throw new DataException("Invalid DNS query encountered");
            }

            var dnsHeader = buffer[..headerBytes].ToArray().Deserialize<DnsHeader>();

            if (dnsHeader.QuestionCount < 1)
            {
                Log.Error("Received invalid DNS query\n{Packet}", buffer.ToHexDump());
                return;
            }

            var question = Question.FromBytes(buffer[headerBytes..].ToArray());

            try
            {
                var resolvedDomainAddress = _domainResolver.ResolveDomain(question.Domain);

                byte[] dnsResponse;
                if (resolvedDomainAddress.Type == ResolverEntry.ResolutionType.Dns)
                {
                    dnsResponse = await _packetForwarder.SendDnsQuery(resolvedDomainAddress, buffer.ToArray());
                }
                else
                {
                    dnsResponse = await _packetForwarder.AnswerQuery(resolvedDomainAddress, dnsHeader, question);
                }

                if (dnsResponse.Length < 1)
                {
                    Log.Error($"Failed to get a valid response message for {question.Domain}");
                    return;
                }

                Log.Verbose("Forwarding DNS response: \n{ResponsePacket}", dnsResponse.ToHexDump());

                await _udpListener.SendToAsync(dnsResponse, ep);
            }
            catch (Exception e)
            {
                Log.Error(e, "Failure while attempting to proxy DNS query");
            }
        }
        catch (ObjectDisposedException)
        {
        }
        catch (Exception e)
        {
            Log.Error(e, "Exception while creating UDP session");
        }
    }
}
