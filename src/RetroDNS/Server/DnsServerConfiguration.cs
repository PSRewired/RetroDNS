namespace RetroDNS.Server;

public class DnsServerConfiguration
{
    public string BindIp { get; set; } = "0.0.0.0";
    public int Port { get; set; } = 53;
}