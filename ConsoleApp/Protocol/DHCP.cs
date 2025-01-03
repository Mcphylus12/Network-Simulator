using ConsoleApp.Protocol.MAC;

namespace ConsoleApp.Protocol;

// https://datatracker.ietf.org/doc/html/rfc2131
internal class DHCP
{
    private static readonly DataPacker _packer = new DataPacker(
    [
            1, // OP
            1, // HTYPE
            1, // HLEN
            1, // HOPS 
            4, // XID 
            2, // SECS 
            2, // FLAGS
            4, // CIADDR 
            4, // YIADDR 
            4, // SIADDR 
            4, // SIADDR 
            16, // Client hardwarre address
            64, // Server Hostname
            128, // Bootfile name
            0 // Options
    ]);

    public byte[] Bytes { get; private set; }

    public DHCP(MacAddress sourceMac)
    {
        Bytes = _packer.Pack(
            [1],
            [1],
            [6],
            [0],
            BitConverter.GetBytes(Random.Shared.Next()),
            [0, 0],
            [0, 0],
            [0, 0, 0, 0],
            [0, 0, 0, 0],
            [0, 0, 0, 0],
            [0, 0, 0, 0],
            sourceMac.ToByteArray()
            );
    }
}
