using System.Collections.Concurrent;
using Tests;

namespace ConsoleApp;
public class CamTable
{
    private ConcurrentDictionary<MacAddress, uint> _portMap = new ConcurrentDictionary<MacAddress, uint>();

    public IEnumerable<uint> Route(uint inboundPort, MacAddress inboundMacAddress, MacAddress outboundMacAddress)
    {
        _portMap[inboundMacAddress] = inboundPort;

        if (_portMap.TryGetValue(outboundMacAddress, out var outboundPort))
        {
            return [outboundPort];
        } 
        else
        {
            return _portMap.Values.Where(p => p != inboundPort);
        }
    }
}
