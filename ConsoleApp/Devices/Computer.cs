using ConsoleApp.Components;
using ConsoleApp.Protocol;
using ConsoleApp.Protocol.MAC;

namespace ConsoleApp.Devices;

internal class Computer : IDevice, IConnectable
{
    public NetworkPort _port;
    private readonly string _name;
    private readonly INetworkLogger _networkLogger;
    private readonly Dictionary<IPAddress, MacAddress> _arpTable;

    public MacAddress MacAddress { get; } = new MacAddress();
    public IPAddress? IPAddress { get; set; }

    public string Name => _name;

    public Computer(string name, INetworkLogger networkLogger)
    {
        _arpTable = new Dictionary<IPAddress, MacAddress>();
        _port = new NetworkPort(DataReceived);
        _name = name;
        _networkLogger = networkLogger;
        _networkLogger.Log(this, NetworkLayer.NA, "PC", MacAddress);
    }

    private void DataReceived(byte[] data)
    {
        _networkLogger.Log(this, NetworkLayer.Raw, "Recieving Raw Data", data);
        try
        {
            var frame = new MACFrame(data);
            var targetAddress = frame.TargetAddress;
            if (targetAddress == MacAddress || targetAddress.IsBroadcast)
            {
                ProcessFrame(frame);
            }
        }
        catch (Exception ex)
        {
            _networkLogger.Log(this, NetworkLayer.Raw, "Error unpacking frame", ex.ToString());
        }
    }

    private void ProcessFrame(MACFrame frame)
    {
        _networkLogger.Log(this, NetworkLayer.Frames, "Recieving Frame", frame);
        if (frame.Type == MACFrame.EtherType.ARP)
        {
            var arpData = new ARPPacket(frame.Payload);
            if (arpData.TargetIp == IPAddress!)
            {
                _arpTable[arpData.SourceIpAddress] = arpData.SourceMacAddress;
                _networkLogger.Log(this, NetworkLayer.Arp, $"Populating arp table {arpData.SourceIpAddress} => {arpData.SourceMacAddress}");

                if (arpData.IsRequest)
                {
                    var response = new ARPPacket(MacAddress, IPAddress!, arpData.SourceMacAddress, arpData.SourceIpAddress);
                    _port.Send(new MACFrame(MacAddress, arpData.SourceMacAddress, MACFrame.EtherType.ARP, response.ToByteArray()).Bytes);
                }
            } 
        }
    }

    public void DebugSend(byte[] data)
    {
        _networkLogger.Log(this, NetworkLayer.Raw, "Sending Raw Data", data);
        _port.Send(data);
    }

    public void DebugSendFrame(MacAddress target, byte[] data)
    {
        var frame = new MACFrame(MacAddress, target, MACFrame.EtherType.Custom, data);
        _networkLogger.Log(this, NetworkLayer.Frames, "Sending Frame", frame);
        DebugSend(frame.Bytes);
    }

    internal void Connect(IConnectable connectable)
    {
        connectable.Connect(_port);
    }

    public void Connect(NetworkPort networkPort)
    {
        _port.ConnectTo(networkPort);
    }

    internal void DebugSendArp(IPAddress targetIp)
    {
        if (IPAddress is null)
        {
            throw new NotSupportedException("Machine needs an ip to send arp requests");
        }

        var arpRequest = new ARPPacket(MacAddress, IPAddress, targetIp);
        var frame = new MACFrame(MacAddress, MacAddress.BroadCastAddress, MACFrame.EtherType.ARP, arpRequest.ToByteArray());
        _port.Send(frame.Bytes);
    }
}

internal interface IDevice
{
    MacAddress MacAddress { get; }
    string Name { get; }
}

internal interface INetworkLogger
{
    void Log(IDevice device, NetworkLayer layer, string message, object? data = null);
}

public enum NetworkLayer
{
    Raw,
    Frames,
    Arp,
    NA
}