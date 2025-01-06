using ConsoleApp;

namespace Tests;
public class EthernetFrameTests
{
    [Fact]
    public void ProcessFrame()
    {
        var sourceAddress = new MacAddress();
        var targetAddress = new MacAddress();

        var frame = new EthernetFrame(sourceAddress, targetAddress, EthernetFrame.EtherType.Custom, [0]);

        var dupeFrame = new EthernetFrame(frame.Bytes);

        dupeFrame.TargetAddress = new MacAddress();

        Assert.True(sourceAddress == dupeFrame.SourceAddress);
    }
}
