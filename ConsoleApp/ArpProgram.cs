using System.Collections.Concurrent;
using Tests;

namespace ConsoleApp;

public sealed class ArpProgram : IDisposable
{
    private readonly IIPCapabilities _device;
    private readonly IDisposable _listenHandle;
    public ConcurrentDictionary<IPAddress, MacAddress> _table = new ConcurrentDictionary<IPAddress, MacAddress>();
    public ConcurrentDictionary<IPAddress, TaskCompletionSource<MacAddress>> _pendingResolutions = new ConcurrentDictionary<IPAddress, TaskCompletionSource<MacAddress>>();

    public ArpProgram(IIPCapabilities device)
    {
        _device = device;
        _table[device.IPAddress] = device.MacAddress;
        _listenHandle = _device.ListenToEthernet(EthernetFrame.EtherType.ARP, HandleArpResponse);
    }

    public void Dispose()
    {
        foreach (var item in _pendingResolutions)
        {
            item.Value.SetCanceled();
        }
        _listenHandle.Dispose();
    }

    private void HandleArpResponse(EthernetFrame ethernetFrame)
    {
        var arpPacket = new ARPPacket(ethernetFrame.Data);

        _table[arpPacket.SourceProtocolAddress] = arpPacket.SourceHardwareAddress;

        if (_pendingResolutions.Remove(arpPacket.SourceProtocolAddress, out var pending))
        {
            pending.TrySetResult(arpPacket.SourceHardwareAddress);
        }

        if (arpPacket.Operation == ARPPacket.REQUEST && arpPacket.TargetProtocolAddress == _device.IPAddress)
        {
            SendArp(ARPPacket.RESPONSE, arpPacket.SourceProtocolAddress, arpPacket.SourceHardwareAddress);
        }
    }

    public Task<MacAddress> GetMacAddress(IPAddress ipAddress, CancellationToken token)
    {
        if (_table.TryGetValue(ipAddress, out var macAddress))
        {
            return Task.FromResult(macAddress);
        }

        var pendingResolution = _pendingResolutions.GetOrAdd(ipAddress, _ => new TaskCompletionSource<MacAddress>());

        token.Register(() =>
        {
            if (_pendingResolutions.Remove(ipAddress, out var pending))
            {
                pending.SetCanceled();
            }
        });

        SendArp(ARPPacket.REQUEST, ipAddress, MacAddress.BroadCastAddress);

        return pendingResolution.Task;
    }

    private void SendArp(ushort type, IPAddress targetIp, MacAddress targetMac)
    {
        var arpRequest = new ARPPacket(type, _device.MacAddress, _device.IPAddress, targetMac, targetIp);
        _device.SendEthernet(targetMac, EthernetFrame.EtherType.ARP, arpRequest.Bytes);
    }
}
