using ConsoleApp.Components;
using ConsoleApp.Protocol.MAC;

namespace ConsoleApp.Devices;

internal class Switch : IDevice
{
    private string _name;
    private INetworkLogger _logger;
    public MacAddress MacAddress { get; set; } = new MacAddress();

    public string Name => _name;

    private NetworkPort[] _ports;

    private Dictionary<MacAddress, uint> _camTable;

    public Switch(string name, INetworkLogger logger, uint numPorts)
    {
        _name = name;
        _logger = logger;
        _ports = new NetworkPort[numPorts];
        _camTable = new Dictionary<MacAddress, uint>();
        _logger.Log(this, NetworkLayer.NA, "Switch Name", name);
    }

    internal void Connect(IConnectable connectable)
    {
        for (uint i = 0; i < _ports.Length; i++)
        {
            if (_ports[i] is null)
            {
                _ports[i] = new NetworkPort(HandleData(i));
                connectable.Connect(_ports[i]);
                return;
            }
        }

        throw new Exception("Switch is full");
    }

    private Action<byte[]> HandleData(uint inboundPortNumber) => data =>
    {
        try
        {
            var frame = new MACFrame(data);
            var source = frame.SourceAddress;
            var target = frame.TargetAddress;

            if (!_camTable.ContainsKey(source))
            {
                _logger.Log(this, NetworkLayer.NA, $"Populated CAM Table {source} -> {inboundPortNumber}", null);
                _camTable[source] = inboundPortNumber;
            }

            if (_camTable.TryGetValue(target, out var outboundPort) && !target.IsBroadcast)
            {
                _ports[outboundPort].Send(data);
            }
            else
            {
                SendBroadcast(inboundPortNumber, data);
            }
        }
        catch (Exception ex)
        {
            _logger.Log(this, NetworkLayer.Raw, "Error unpacking frame", ex.ToString());
        }
    };

    private void SendBroadcast(uint inboundPortNumber, byte[] data)
    {
        for (uint i = 0; i < _ports.Length; i++)
        {
            if (_ports[i] is not null && i != inboundPortNumber)
            {
                _ports[i].Send(data);
            }
        }
    }
}

internal interface IConnectable
{
    void Connect(NetworkPort networkPort);
}