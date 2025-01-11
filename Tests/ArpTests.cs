using ConsoleApp;

namespace Tests;
public class ArpTests
{
    [Fact]
    public async Task ToDevicesTalk()
    {
        var ct = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        var dev1 = new ArpTestDevice(new IPAddress("0.0.0.1"), new MacAddress());
        var arp1 = new ArpProgram(dev1);

        var dev2 = new ArpTestDevice(new IPAddress("0.0.0.2"), new MacAddress());
        var arp2 = new ArpProgram(dev2);

        dev1.Connect(dev2);

        var actualMac2 = await arp1.GetMacAddress(dev2.IPAddress, ct.Token);
        var actualMac1 = await arp2.GetMacAddress(dev1.IPAddress, ct.Token);
        Assert.Equal(dev2.MacAddress, actualMac2);
        Assert.Equal(dev1.MacAddress, actualMac1);
    }
}

public class ArpTestDevice : IEthernetCapabilities, IIPCapabilities
{
    public ArpTestDevice(IPAddress iPAddress, MacAddress macAddress)
    {
        IPAddress = iPAddress;
        MacAddress = macAddress;
    }

    public IPAddress IPAddress { get; }
    public MacAddress MacAddress { get; }
    public ArpTestDevice? _other;
    public Action<EthernetFrame>? _listener;

    public IDisposable ListenToEthernet(EthernetFrame.EtherType type, Action<EthernetFrame> handleArpResponse)
    {
        _listener = handleArpResponse;
        return new TestDispose();
    }

    public void SendEthernet(MacAddress targetMac, EthernetFrame.EtherType type, byte[] data)
    {
        var frame = new EthernetFrame(MacAddress, targetMac, type, data);
        Task.Factory.StartNew(o => ((ArpTestDevice?)o)?._listener?.Invoke(frame), _other);
    }

    internal void Connect(ArpTestDevice dev2)
    {
        _other = dev2;
        dev2._other = this;
    }

    public IDisposable ListenToIP(IPPacket.IPProtocol type, Action<IPPacket> handleIpPacket)
    {
        throw new NotImplementedException();
    }

    public void SendIP(IPAddress targetMac, IPPacket.IPProtocol type, byte[] data)
    {
        throw new NotImplementedException();
    }

    private class TestDispose : IDisposable
    {
        public void Dispose()
        {
        }
    }
}
