using ConsoleApp;

namespace Tests;
public class UDPPacketTests
{
    [Fact]
    public void ProcessFrame()
    {
        var packet = new UDPPacket(2000, 2300, [0]);

        var dupePacket = new UDPPacket(packet.Bytes);

        dupePacket.TargetPort = 15;

        Assert.True(2000 == dupePacket.SourcePort);
    }
}
