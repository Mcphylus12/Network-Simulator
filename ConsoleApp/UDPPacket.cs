namespace ConsoleApp;

// https://datatracker.ietf.org/doc/html/rfc768
// https://en.wikipedia.org/wiki/User_Datagram_Protocol
public class UDPPacket
{
    private readonly static BytePacker _packer = new BytePacker(new List<BytePacker.PackerSection>
    {
        new BytePacker.PackerSection<ushort>("SourcePort", 2, BitConverter.GetBytes, b => BitConverter.ToUInt16(b, 0)),
        new BytePacker.PackerSection<ushort>("TargetPort", 2, BitConverter.GetBytes, b => BitConverter.ToUInt16(b, 0)),
        new BytePacker.PackerSection<byte[]>("Data", 0),
    });

    public byte[] Bytes { get; }
    public ushort SourcePort { get => _packer.Get<ushort>(Bytes, "SourcePort"); set => _packer.Set(Bytes, "SourcePort", value); }
    public ushort TargetPort { get => _packer.Get<ushort>(Bytes, "TargetPort"); set => _packer.Set(Bytes, "TargetPort", value); }
    public byte[] Data { get => _packer.Get<byte[]>(Bytes, "Data"); set => _packer.Set(Bytes, "Data", value); }

    public UDPPacket(byte[] bytes)
    {
        Bytes = new byte[bytes.Length];
        bytes.CopyTo(Bytes, 0);
    }

    public UDPPacket(ushort sourcePort, ushort targetPort, byte[] payload)
    {
        Bytes = _packer.CreatePacket((uint)payload.Length);

        _packer.Set(Bytes, "SourcePort", sourcePort);
        _packer.Set(Bytes, "TargetPort", targetPort);
        _packer.Set(Bytes, "Data", payload);
    }
}
