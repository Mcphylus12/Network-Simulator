using Tests;

namespace ConsoleApp;

// https://en.wikipedia.org/wiki/Ethernet_frame
public class EthernetFrame
{
    private readonly static BytePacker _packer = new BytePacker(new List<BytePacker.PackerSection>
    {
        new BytePacker.PackerSection<MacAddress>("SourceAddress", MacAddress.LENGTH, m => m.ToByteArray(), b => new MacAddress(b)),
        new BytePacker.PackerSection<MacAddress>("TargetAddress", MacAddress.LENGTH, m => m.ToByteArray(), b => new MacAddress(b)),
        new BytePacker.PackerSection<EtherType>("Type", 2, t => BitConverter.GetBytes((ushort)t), b => (EtherType)BitConverter.ToUInt16(b)),
        new BytePacker.PackerSection<byte[]>("Data", 0),
    });

    public byte[] Bytes { get; }
    public MacAddress SourceAddress { get => _packer.Get<MacAddress>(Bytes, "SourceAddress"); set => _packer.Set(Bytes, "SourceAddress", value); }
    public MacAddress TargetAddress { get => _packer.Get<MacAddress>(Bytes, "TargetAddress"); set => _packer.Set(Bytes, "TargetAddress", value); }
    public EtherType Type { get => _packer.Get<EtherType>(Bytes, "Type"); set => _packer.Set(Bytes, "Type", value); }
    public byte[] Data { get => _packer.Get<byte[]>(Bytes, "Data"); set => _packer.Set(Bytes, "Data", value); }

    public EthernetFrame(byte[] bytes)
    {
        Bytes = new byte[bytes.Length];
        bytes.CopyTo(Bytes, 0);
    }

    public EthernetFrame(MacAddress sourceAddress, MacAddress targetAddress, EtherType type, byte[] payload)
    {
        Bytes = _packer.CreatePacket((uint)payload.Length);

        _packer.Set(Bytes, "SourceAddress", sourceAddress);
        _packer.Set(Bytes, "TargetAddress", targetAddress);
        _packer.Set(Bytes, "Type", type);
        _packer.Set(Bytes, "Data", payload);
    }

    public enum EtherType
    {
        Custom = 0x0000,
        IPv4 = 0x0800,
        ARP = 0x0806,
    }
}
