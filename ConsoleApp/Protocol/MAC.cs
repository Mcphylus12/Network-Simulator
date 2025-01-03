namespace ConsoleApp.Protocol.MAC;

// https://en.wikipedia.org/wiki/Ethernet_frame
internal class MACFrame : IDebugString
{
    private static readonly DataPacker _packer = new DataPacker([MacAddress.LENGTH, MacAddress.LENGTH, 2, 0]);
    public byte[] Bytes { get; private set; }

    public MACFrame(byte[] bytes)
    {
        Bytes = bytes;
    }

    public MACFrame(MacAddress sourceAddress, MacAddress targetAddress, EtherType type, byte[] payload)
    {
        Bytes = _packer.Pack(sourceAddress.ToByteArray(), targetAddress.ToByteArray(), BitConverter.GetBytes((Half)(int)type), payload);
    }

    public MacAddress SourceAddress => _packer.GetField(Bytes, 0, b => new MacAddress(b));
    public MacAddress TargetAddress => _packer.GetField(Bytes, 1, b => new MacAddress(b));
    public EtherType Type => _packer.GetField(Bytes, 2, b => (EtherType)(int)BitConverter.ToHalf(b));
    public byte[] Payload => _packer.GetField(Bytes, 3, b => b);

    public string ToDebugString()
    {
        return 
@$"
---------------------
Source: {SourceAddress} 
Target: {TargetAddress}
---------------------";
    }

    public enum EtherType
    {
        Custom = 0x0000,
        IPv4 = 0x0800,
        ARP = 0x0806,
    }
}


public class MacAddress : IDebugString
{
    private byte[] _data = new byte[6];

    public const int LENGTH = 6;

    public bool IsBroadcast => _data.All(b => b == 255);

    public readonly static MacAddress BroadCastAddress = new MacAddress([255, 255, 255, 255, 255, 255]);
    public readonly static MacAddress EmptyAddress = new MacAddress([0, 0, 0, 0, 0, 0]);

    public MacAddress()
    {
        Random.Shared.NextBytes(_data);
    }

    public MacAddress(byte[] bytes)
    {
        if (bytes.Length != 6) throw new NotSupportedException("Mac addresses should be length 6");

        Array.Copy(bytes, _data, 6);
    }

    internal byte[] ToByteArray()
    {
        return _data;
    }

    public override bool Equals(object? obj)
    {
        return obj is MacAddress other && Enumerable.SequenceEqual(other._data, _data);
    }
    public override int GetHashCode()
    {
        return _data.Aggregate(0, (a, b) => HashCode.Combine(a, b.GetHashCode()));
    }


    public static bool operator ==(MacAddress b1, MacAddress b2) => b1.Equals(b2);
    public static bool operator !=(MacAddress b1, MacAddress b2) => !b1.Equals(b2);

    public override string ToString()
    {
        return string.Join(':', _data.Select(b => b.ToString()));
    }

    public string ToDebugString() => ToString();
}