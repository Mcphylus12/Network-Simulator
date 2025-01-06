using ConsoleApp;

namespace Tests;
public class IPPacketTests
{
    [Fact]
    public void ProcessPacket()
    {
        var sourceAddress = new IPAddress("10.0.0.0");
        var targetAddress = new IPAddress("10.0.0.1");

        var packet = new IPPacket(sourceAddress, targetAddress, IPPacket.IPProtocol.Debug, [0]);

        var dupePacket = new IPPacket(packet.Bytes);

        dupePacket.TargetAddress = new IPAddress("10.0.0.2");

        Assert.True(sourceAddress == dupePacket.SourceAddress);
    }
}
