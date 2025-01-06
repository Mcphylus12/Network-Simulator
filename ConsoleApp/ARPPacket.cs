using Tests;

namespace ConsoleApp;

// https://datatracker.ietf.org/doc/html/rfc6747
public class ARPPacket
{
    private readonly static BytePacker _packer = new BytePacker(new List<BytePacker.PackerSection>
    {
        new BytePacker.PackerSection<ushort>("Operation", 2, BitConverter.GetBytes, b => BitConverter.ToUInt16(b, 0)),
        new BytePacker.PackerSection<MacAddress>("SourceHardwareAddress", MacAddress.LENGTH, m => m.ToByteArray(), b => new MacAddress(b)),
        new BytePacker.PackerSection<IPAddress>("SourceProtocolAddress", IPAddress.LENGTH, m => m.ToByteArray(), b => new IPAddress(b)),
        new BytePacker.PackerSection<MacAddress>("TargetHardwareAddress", MacAddress.LENGTH, m => m.ToByteArray(), b => new MacAddress(b)),
        new BytePacker.PackerSection<IPAddress>("TargetProtocolAddress", IPAddress.LENGTH, m => m.ToByteArray(), b => new IPAddress(b)),
    });

    public byte[] Bytes { get; }
    public ushort Operation { get => _packer.Get<ushort>(Bytes, "Operation"); set => _packer.Set(Bytes, "Operation", value); }
    public MacAddress SourceHardwareAddress { get => _packer.Get<MacAddress>(Bytes, "SourceHardwareAddress"); set => _packer.Set(Bytes, "SourceHardwareAddress", value); }
    public IPAddress SourceProtocolAddress { get => _packer.Get<IPAddress>(Bytes, "SourceProtocolAddress"); set => _packer.Set(Bytes, "SourceProtocolAddress", value); }
    public MacAddress TargetHardwareAddress { get => _packer.Get<MacAddress>(Bytes, "TargetHardwareAddress"); set => _packer.Set(Bytes, "TargetHardwareAddress", value); }
    public IPAddress TargetProtocolAddress { get => _packer.Get<IPAddress>(Bytes, "TargetProtocolAddress"); set => _packer.Set(Bytes, "TargetProtocolAddress", value); }

    public ARPPacket(byte[] bytes)
    {
        Bytes = new byte[bytes.Length];
        bytes.CopyTo(Bytes, 0);
    }

    public ARPPacket(MacAddress sourceHardwareAddress, IPAddress sourceProtocolAddress, MacAddress targetHardwareAddress, IPAddress targetProtocolAddress)
    {
        Bytes = _packer.CreatePacket();

        _packer.Set(Bytes, "SourceHardwareAddress", sourceHardwareAddress);
        _packer.Set(Bytes, "SourceProtocolAddress", sourceProtocolAddress);
        _packer.Set(Bytes, "TargetHardwareAddress", targetHardwareAddress);
        _packer.Set(Bytes, "TargetProtocolAddress", targetProtocolAddress);
    }
}
