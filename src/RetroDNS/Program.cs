using Microsoft.Extensions.DependencyInjection;
using RetroDNS.Forwarders;
using RetroDNS.Resolvers;
using RetroDNS.Server;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Verbose()
    .WriteTo.Console()
    .CreateLogger();

var serviceCollection = new ServiceCollection();
serviceCollection.Configure<DnsServerConfiguration>(o =>
{
    o.BindIp = "192.168.1.52";
});

serviceCollection.AddSingleton<DomainCacheResolver>();
serviceCollection.AddScoped<DnsPacketForwarder>();
serviceCollection.AddSingleton<DnsServer>();

var services = serviceCollection.BuildServiceProvider();

var dnsResolver = services.GetRequiredService<DomainCacheResolver>();

var domainsFile = "./domains.txt";

Log.Information("Registering domains from file: {FileName}", domainsFile);
await dnsResolver.RegisterDomainsFromFile("./domains.txt");

Log.Information("Starting RetroDNS");
var server = services.GetRequiredService<DnsServer>();

Console.CancelKeyPress += (_, _) =>
{
    server.Stop();
};
server.Start();

Console.ReadLine();
server.Stop();
Log.CloseAndFlush();
