using ConsoleApp.Components;
using ConsoleApp.Protocol;
using ConsoleApp.Protocol.MAC;
using System.Text;

namespace ConsoleApp.Devices;

internal class Computer : IDevice, IConnectable
{
    public NetworkPort _port;
    private readonly string _name;
    private readonly INetworkLogger _networkLogger;
    private readonly Dictionary<IPAddress, MacAddress> _arpTable;
    private readonly Dictionary<ushort, Action<byte[]>> _portBindings;

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
        _portBindings = new Dictionary<ushort, Action<byte[]>>();
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
            ProcessArp(arpData);
        }
        else if (frame.Type == MACFrame.EtherType.IPv4)
        {
            var ipData = new IPPacket(frame.Payload);
            ProcessIP(ipData);
        }
    }

    private void ProcessIP(IPPacket ipData)
    {
        _networkLogger.Log(this, NetworkLayer.IP, "Recieving IP packet", ipData);
        if (ipData.Protocol == IPPacket.IPProtocol.Debug)
        {
            var data = Encoding.UTF8.GetString(ipData.PayLoad);
            _networkLogger.Log(this, NetworkLayer.NA, data);
        }
        else if (ipData.Protocol == IPPacket.IPProtocol.UDP)
        {
            var udp = new UDPPacket(ipData.PayLoad);
            var port = udp.TargetPort;

            if (_portBindings.TryGetValue(port, out var handler))
            {
                handler(udp.Payload);
            }
        }
    }

    private void ProcessArp(ARPPacket arpData)
    {
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

    public void Send(byte[] data)
    {
        _networkLogger.Log(this, NetworkLayer.Raw, "Sending Raw Data", data);
        _port.Send(data);
    }

    public void SendFrame(MacAddress target, byte[] data, MACFrame.EtherType type = MACFrame.EtherType.Custom)
    {
        var frame = new MACFrame(MacAddress, target, type, data);
        _networkLogger.Log(this, NetworkLayer.Frames, "Sending Frame", frame);
        Send(frame.Bytes);
    }

    internal void Connect(IConnectable connectable)
    {
        connectable.Connect(_port);
    }

    public void Connect(NetworkPort networkPort)
    {
        _port.ConnectTo(networkPort);
    }

    internal void SendArp(IPAddress targetIp)
    {
        if (IPAddress is null)
        {
            throw new NotSupportedException("Machine needs an ip to send arp requests");
        }

        var arpRequest = new ARPPacket(MacAddress, IPAddress, targetIp);
        SendFrame(MacAddress.BroadCastAddress, arpRequest.ToByteArray(), MACFrame.EtherType.ARP);
    }

    internal async Task SendIP(IPAddress targetIp, byte[] data, IPPacket.IPProtocol type = IPPacket.IPProtocol.Debug)
    {
        if (IPAddress is null)
        {
            throw new NotSupportedException("Machine needs an ip to send IP requests");
        }

        if (!_arpTable.ContainsKey(targetIp))
        {
            SendArp(targetIp);
        }

        int retryCount = 5;
        while (retryCount >= 0)
        {
            if (_arpTable.TryGetValue(targetIp, out var macAddress))
            {
                var ipPacket = new IPPacket(IPAddress, targetIp, type, data);
                SendFrame(macAddress, ipPacket.Bytes, MACFrame.EtherType.IPv4);
                break;
            }

            await Task.Delay(TimeSpan.FromSeconds(1));

            retryCount--;
        }
    }

    internal Task SendUDP(IPAddress targetIp, ushort port, byte[] data)
    {
        var udp = new UDPPacket(0, port, data);
        return SendIP(targetIp, udp.Bytes, IPPacket.IPProtocol.UDP);
    }

    public void Bind(ushort port, Action<byte[]> handler)
    {
        if (_portBindings.ContainsKey(port))
        {
            throw new Exception("Bnding to a bound port");
        }

        _portBindings[port] = handler;
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
    IP,
    Port,
    NA
}