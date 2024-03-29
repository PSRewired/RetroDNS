using Serilog;

namespace RetroDNS.Resolvers;

public class DomainCacheResolver
{
    private static readonly Dictionary<string, ResolverEntry> _registeredDomains = new();

    private static ILogger Log => Serilog.Log.ForContext<DomainCacheResolver>();

    /// <summary>
    /// Searches the given directory for domain files and loads them using <see cref="RegisterDomainsFromFile"/>
    /// </summary>
    /// <param name="directory"></param>
    public async Task RegisterDomainsFromDirectory(string directory)
    {
        _registeredDomains.Clear();

        var files = Directory.EnumerateFiles(directory, "*domains*.txt", SearchOption.TopDirectoryOnly)
            .OrderBy(f => f);

        foreach (var file in files)
        {
            Log.Information("Loading domains from file: {FileName}", file);
            await RegisterDomainsFromFile(file);
        }
    }

    /// <summary>
    /// Registers all routes from a file
    /// The expected format is 'domain=dnsIp' given one per line
    /// </summary>
    /// <param name="filePath"></param>
    public async Task RegisterDomainsFromFile(string filePath)
    {
        var file = await File.ReadAllLinesAsync(filePath);
        var lineNumber = 0;

        foreach (var route in file)
        {
            // Skip comment lines
            if (route.StartsWith('#') || route.StartsWith("//"))
            {
                continue;
            }

            lineNumber++;
            var routeParts = route.Split('=');

            if (routeParts.Length != 2)
            {
                Log.Error("Failed to parse line {LineNumber}. Expected format is url=ip, got {ActualValue}", route);
                continue;
            }

            var domain = routeParts[0].Trim();
            var dnsServerIp = routeParts[1].Trim();

            if (_registeredDomains.ContainsKey(domain))
            {
                Log.Warning(
                    "Duplicate entry for {Domain} in file {FileName} with value of {DnsServerIp}. Line {LineNumber} with value of {LineValue} has been skipped!",
                    domain, filePath, dnsServerIp, lineNumber, route);
                continue;
            }

            Log.Debug("[{LineNumber}]: Added route {Route}", lineNumber, route);
            AddDomain(domain, dnsServerIp);
        }
    }

    /// <summary>
    /// Adds a domain entry to the lookup table
    /// </summary>
    /// <param name="domainName"></param>
    /// <param name="dnsServerIp"></param>
    public void AddDomain(string domainName, string dnsServerIp)
    {
        _registeredDomains[domainName] = ResolverEntry.FromFqdn(dnsServerIp);
    }

    /// <summary>
    /// Looks for the given domain and any other possible wildcard domains that are defined in the lookup table
    /// </summary>
    /// <param name="domainName">A domain name or IP address to find in the lookup table</param>
    /// <returns>IPEndpoint that represents the configured DNS server for the matched entry</returns>
    /// <exception cref="ArgumentException">When no variant of the domain was found</exception>
    public ResolverEntry ResolveDomain(string domainName)
    {
        var matchableDomainNames = BuildWildcardDomains(domainName);

        foreach (var domain in matchableDomainNames)
        {
            if (!_registeredDomains.TryGetValue(domain, out var dnsServer))
            {
                continue;
            }

            Log.Debug("Matched resolution entry for {Domain}: {Entry}", domain, dnsServer);
            return dnsServer;
        }

        throw new ArgumentException($"Could not find a registered domain for {domainName}");
    }

    private static string[] BuildWildcardDomains(string domain)
    {
        Log.ForContext<DomainCacheResolver>().Verbose("Building wildcard domains for: {Domain}", domain);

        // Hack to make the C# URI parser work
        if (!domain.StartsWith("http"))
        {
            domain = $"http://{domain}";
        }

        var uri = new Uri(domain);
        var host = uri.DnsSafeHost;

        if (uri.HostNameType is UriHostNameType.IPv4 or UriHostNameType.IPv6)
        {
            return new[] { host };
        }

        var hostParts = host.Split('.');

        var domains = new string[hostParts.Length];
        domains[0] = host;
        domains[hostParts.Length - 1] = "*";

        for (var i = 1; i < domains.Length - 1; i++)
        {
            domains[i] = $"*.{string.Join('.', hostParts[i..])}";
            Log.Verbose("\t[{VariantNumber}]: {GeneratedDomain}", i, domains[i]);
        }

        return domains;
    }
}
