using Tests;

namespace ConsoleApp;

public interface IEthernetCapabilities
{
    MacAddress MacAddress { get; }

    IDisposable ListenToEthernet(EthernetFrame.EtherType type, Action<EthernetFrame> handleFrame);
    void SendEthernet(MacAddress targetMac, EthernetFrame.EtherType type, byte[] data);
}

public interface IIPCapabilities : IEthernetCapabilities
{
    IPAddress IPAddress { get; }
    IDisposable ListenToIP(IPPacket.IPProtocol type, Action<IPPacket> handleIpPacket);
    void SendIP(IPAddress targetMac, IPPacket.IPProtocol type, byte[] data);
}
