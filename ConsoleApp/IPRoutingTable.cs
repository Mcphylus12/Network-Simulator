
namespace ConsoleApp;
public class IPRoutingTable
{
    public SortedList<uint, RouteEntry> _entries;

    public IPRoutingTable()
    {
        _entries = new SortedList<uint, RouteEntry>();
    }

    public void AddEntry(RouteEntry routeEntry)
    {
        _entries.Add(routeEntry.Metric, routeEntry);
    }

    public IPAddress? Route(IPAddress destIp)
    {
        uint destIpInt = BitConverter.ToUInt32(destIp.ToByteArray());

        foreach (var item in _entries)
        {
            var netIpInt = BitConverter.ToUInt32(item.Value.NetAddr.ToByteArray());
            var subnetIpInt = BitConverter.ToUInt32(item.Value.Subnet.ToByteArray());
            var netAddr = netIpInt & subnetIpInt;
            var targetNet = destIpInt & subnetIpInt;

            var match = (netAddr ^ targetNet) == 0;

            if (match)
            {
                return item.Value.TargetIP;
            }
        }

        return null;
    }

    public record class RouteEntry(IPAddress NetAddr, IPAddress Subnet, IPAddress TargetIP, uint Metric);
}
