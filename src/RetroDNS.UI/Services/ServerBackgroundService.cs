using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using RetroDNS.Forwarders;
using RetroDNS.Resolvers;
using RetroDNS.Server;

namespace RetroDNS.UI.Services;

public class ServerBackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;

    private IServiceScope? _currentScope;
    private DnsServer? _dnsServer;

    public bool IsActive => _dnsServer != null;

    public ServerBackgroundService(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    public void Start(string bindingIp)
    {
        _currentScope = _scopeFactory.CreateScope();

        var packetForwarder = _currentScope.ServiceProvider.GetRequiredService<DnsPacketForwarder>();
        var domainResolver = _currentScope.ServiceProvider.GetRequiredService<DomainCacheResolver>();

        var server = new DnsServer(Options.Create(new DnsServerConfiguration
        {
            BindIp = bindingIp,
        }), packetForwarder, domainResolver);

        _dnsServer = server;

        server.Start();
    }

    public void Stop()
    {
        _dnsServer?.Stop();
        _currentScope?.Dispose();

        _dnsServer = null;
        _currentScope = null;
    }
}
