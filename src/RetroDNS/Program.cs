using Microsoft.Extensions.DependencyInjection;
using RetroDNS.Forwarders;
using RetroDNS.Resolvers;
using RetroDNS.Server;
using Serilog;


Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Verbose()
    .WriteTo.Console()
    .CreateLogger();

if (args.Length < 1)
{
    Log.Fatal("Invalid or missing bind IP. Usage: ./RetroDNS x.x.x.x");
    Environment.Exit(1);
}

var serviceCollection = new ServiceCollection();
serviceCollection.Configure<DnsServerConfiguration>(o =>
{
    o.BindIp = args[0];
});

serviceCollection.AddSingleton<DomainCacheResolver>();
serviceCollection.AddScoped<DnsPacketForwarder>();
serviceCollection.AddSingleton<DnsServer>();

var services = serviceCollection.BuildServiceProvider();

var dnsResolver = services.GetRequiredService<DomainCacheResolver>();

await dnsResolver.RegisterDomainsFromDirectory(".");

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
