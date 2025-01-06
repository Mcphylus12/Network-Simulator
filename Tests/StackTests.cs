using ConsoleApp;

namespace Tests;

public class StackTests
{
    [Fact]
    public void UDPToFrame()
    {
        ushort sourcePort = 1, targetPort = 2;
        IPAddress sourceIp = new IPAddress("0.0.0.0"), targetIp = new IPAddress("10.0.0.1");
        MacAddress sourceMac = new MacAddress(), targetMac = new MacAddress();
        byte[] data = [1];
        var udp = new UDPPacket(sourcePort, targetPort, data);
        var ip = new IPPacket(sourceIp, targetIp, IPPacket.IPProtocol.UDP, udp.Bytes);
        var frame = new EthernetFrame(sourceMac, targetMac, EthernetFrame.EtherType.IPv4, ip.Bytes);

        var recFrame = new EthernetFrame(frame.Bytes);
        var recIpPacket = new IPPacket(recFrame.Data);
        var recUDP = new UDPPacket(recIpPacket.Data);


        Assert.Equivalent(data, recUDP.Data, strict: true);
    }
}