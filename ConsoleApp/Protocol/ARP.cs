using ConsoleApp.Protocol.MAC;

namespace ConsoleApp.Protocol;

// https://datatracker.ietf.org/doc/html/rfc6747
internal class ARPPacket
{
    private static readonly BytePacker _dataPacker = new BytePacker([2, 2, 1, 1, 2, MacAddress.LENGTH, IPAddress.LENGTH, MacAddress.LENGTH, IPAddress.LENGTH]);
    private byte[] _data;

    public ARPPacket(byte[] payload)
    {
        _data = payload;
    }

    public ARPPacket(MacAddress sourceMacAddress, IPAddress sourceIp, IPAddress targetIp)
    {
        byte[] HW_TYPE = [0x00, 0x01];
        byte[] PTYPE = [0x08, 0x00];
        byte[] REQUEST = [0x00, 0x01];

        _data = _dataPacker.Pack
        (
            HW_TYPE,
            PTYPE,
            [MacAddress.LENGTH],
            [IPAddress.LENGTH],
            REQUEST,
            sourceMacAddress.ToByteArray(),
            sourceIp.ToByteArray(),
            MacAddress.EmptyAddress.ToByteArray(),
            targetIp.ToByteArray()
        );
    }

    public ARPPacket(MacAddress sourceMacAddress, IPAddress sourceIp, MacAddress targetMacAddress, IPAddress targetIp)
    {
        byte[] HW_TYPE = [0x00, 0x01];
        byte[] PTYPE = [0x08, 0x00];
        byte[] RESPONSE = [0x00, 0x02];

        _data = _dataPacker.Pack
        (
            HW_TYPE,
            PTYPE,
            [MacAddress.LENGTH],
            [IPAddress.LENGTH],
            RESPONSE,
            sourceMacAddress.ToByteArray(),
            sourceIp.ToByteArray(),
            targetMacAddress.ToByteArray(),
            targetIp.ToByteArray()
        );
    }

    public bool IsRequest
    {
        get
        {
            return _data[7] == 1;
        }
    }

    public MacAddress TargetMacAddress => new MacAddress(_data[18..24]);
    public IPAddress TargetIp => new IPAddress(_data[24..28]);
    public MacAddress SourceMacAddress => new MacAddress(_data[8..14]);
    public IPAddress SourceIpAddress => new IPAddress(_data[14..18]);

    internal byte[] ToByteArray()
    {
        return _data;
    }
}
