using Tests;

namespace ConsoleApp;

// https://datatracker.ietf.org/doc/html/rfc791
// https://en.wikipedia.org/wiki/IPv4#Header
public class IPPacket
{
    private readonly static BytePacker _packer = new BytePacker(new List<BytePacker.PackerSection>
    {
        new BytePacker.PackerSection<IPAddress>("SourceAddress", IPAddress.LENGTH, m => m.ToByteArray(), b => new IPAddress(b)),
        new BytePacker.PackerSection<IPAddress>("TargetAddress", IPAddress.LENGTH, m => m.ToByteArray(), b => new IPAddress(b)),
        new BytePacker.PackerSection<IPProtocol>("Type", 1, t => [(byte)t], t => (IPProtocol)t[0]),
        new BytePacker.PackerSection<byte[]>("Data", 0),
    });

    public byte[] Bytes { get; }
    public IPAddress SourceAddress { get => _packer.Get<IPAddress>(Bytes, "SourceAddress"); set => _packer.Set(Bytes, "SourceAddress", value); }
    public IPAddress TargetAddress { get => _packer.Get<IPAddress>(Bytes, "TargetAddress"); set => _packer.Set(Bytes, "TargetAddress", value); }
    public IPProtocol Type { get => _packer.Get<IPProtocol>(Bytes, "Type"); set => _packer.Set(Bytes, "Type", value); }
    public byte[] Data { get => _packer.Get<byte[]>(Bytes, "Data"); set => _packer.Set(Bytes, "Data", value); }

    public IPPacket(byte[] bytes)
    {
        Bytes = new byte[bytes.Length];
        bytes.CopyTo(Bytes, 0);
    }

    public IPPacket(IPAddress sourceAddress, IPAddress targetAddress, IPProtocol type, byte[] payload)
    {
        Bytes = _packer.CreatePacket((uint)payload.Length);

        _packer.Set(Bytes, "SourceAddress", sourceAddress);
        _packer.Set(Bytes, "TargetAddress", targetAddress);
        _packer.Set(Bytes, "Type", type);
        _packer.Set(Bytes, "Data", payload);
    }

    public enum IPProtocol
    {
        Debug = 254,
        UDP = 17
    }
}
