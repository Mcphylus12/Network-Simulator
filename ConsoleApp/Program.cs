// See https://aka.ms/new-console-template for more information
using ConsoleApp.Devices;
using ConsoleApp.Protocol;
using ConsoleApp.Protocol.MAC;
using System.Text;

var logger = new ConsoleLogger((d, n) => n >= NetworkLayer.Arp);

var @switch = new Switch("Switch1", logger, 3);
var pc1 = new Computer("PC1", logger);
pc1.IPAddress = new IPAddress("192.168.1.1");
var pc2 = new Computer("PC2", logger);
pc2.IPAddress = new IPAddress("192.168.1.2");
var pc3 = new Computer("PC3", logger);

@switch.Connect(pc1);
@switch.Connect(pc2);
@switch.Connect(pc3);

//pc1.DebugSend("This is a message From PC1"u8.ToArray());
//pc2.DebugSend("Hello From PC2"u8.ToArray());
//pc1.DebugSendFrame(pc2.MacAddress, "This is a message From PC1"u8.ToArray());
//pc1.DebugSendArp(pc2.IPAddress);
//await pc1.SendIP(pc2.IPAddress, "This is an IP message From PC1"u8.ToArray());

pc2.Bind(15, b =>
{
    logger.Log(pc2, NetworkLayer.Port, Encoding.UTF8.GetString(b));
});
await pc1.SendUDP(pc2.IPAddress, 15, "This is a UDP message From PC1"u8.ToArray());

Console.ReadLine();

class ConsoleLogger : INetworkLogger
{
    private readonly Func<IDevice, NetworkLayer, bool> _predicate;
    private readonly Dictionary<Type, Func<object, string>> _formatters;


    public ConsoleLogger(Func<IDevice, NetworkLayer, bool> predicate)
    {
        _predicate = predicate;
        _formatters = new Dictionary<Type, Func<object, string>>()
        {
            [typeof(byte[])] = o => Encoding.UTF8.GetString((byte[])o),
            [typeof(string)] = o => (string)o
        };
    }

    public void Log(IDevice device, NetworkLayer layer, string message, object? data = null)
    {
        if (_predicate(device, layer))
        {
            var payload = data switch
            {
                null => string.Empty,
                _ when _formatters.ContainsKey(data.GetType()) => ": " + _formatters[data.GetType()](data),
                IDebugString ds => ": " + ds.ToDebugString(),
                _ => $": No formatter for {data?.GetType()}"
            };
            Console.WriteLine($"[{device.Name}] ({layer}) {message}{payload}");
        }
    }
}
