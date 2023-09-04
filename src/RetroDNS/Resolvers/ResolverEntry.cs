using System.Net;

namespace RetroDNS.Resolvers;

public class ResolverEntry
{
    public enum ResolutionType
    {
        Ip,
        Dns,
    }

    public required ResolutionType Type { get; set; }
    public required IPAddress Host { get; set; }

    public static ResolverEntry FromFqdn(string domain)
    {
        if (!domain.Contains("://"))
        {
            domain = $"http://{domain}";
        }

        var uri = new Uri(domain);

        return new ResolverEntry
        {
            Type = uri.Scheme == "dns" ? ResolutionType.Dns : ResolutionType.Ip,
            Host = IPAddress.Parse(uri.Host),
        };
    }

    public override string ToString()
    {
        return $"{Type.ToString()} -> {Host}";
    }
}
