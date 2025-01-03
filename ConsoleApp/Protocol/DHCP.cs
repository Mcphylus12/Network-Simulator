namespace ConsoleApp.Protocol;

// https://datatracker.ietf.org/doc/html/rfc2131
internal class DHCP
{
    private static readonly NamedBytePacker _packer = new NamedBytePacker(new Dictionary<string, int>()
    {
            ["OP"] = 1,
            ["HTYPE"] = 1,
            ["HLEN"] = 1,
            ["HOPS"] = 1,
            ["XID"] = 4,
            ["SECS"] = 2,
            ["FLAGS"] = 2,
            ["CIADDR"] = 4,
            ["YIADDRE"] = 4,
            ["SIADDRE"] = 4,
            ["GIADDR"] = 4,
            ["CHADDR"] = 16,
            ["SNAME"] = 64,
            ["FILE"] = 128,
            ["OPTIONS"] = 0
    });

    public byte[] Bytes { get; private set; }

    public DHCP()
    {
        var options = new byte[0];

        Bytes = new byte[_packer.Length + options.Length];
    }
}
