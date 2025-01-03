using ConsoleApp.Protocol.MAC;
using static ConsoleApp.Protocol.MAC.MACFrame;

namespace ConsoleApp.Protocol;

// https://datatracker.ietf.org/doc/html/rfc791
internal class IPPacket
{
    private static readonly DataPacker _packer = new DataPacker(
    [
        1, // Ver & IHL
        1, // DSCP & ECN
        2, // TotalLength
        2, // Identification
        2, // Flags & Fragment Offset
        1, // TTL
        1, // Protocol
        2, // Header Checksum
        4, // Source
        4, // Dest
        0  // Data
    ]);

    public byte[] Bytes { get; private set; }

    public IPPacket(IPAddress source, IPAddress dest, IPProtocol protocol, byte[] data)
    {
        byte[] noOptionsFirstByte = [0b0100_0101];
        byte[] dscpAndECN = [0];
        byte[] totalLength = BitConverter.GetBytes((ushort)(20 + data.Length));
        byte[] identificication = [0, 0];
        byte[] flagsAndFragmentOffset = [0, 0];
        byte[] ttl = [255];
        byte[] protocolBytes = [(byte)protocol];
        byte[] checkSum = [0, 0];

        Bytes = _packer.Pack(
            noOptionsFirstByte,
            dscpAndECN,
            totalLength,
            identificication,
            flagsAndFragmentOffset,
            ttl,
            protocolBytes,
            checkSum,
            source.ToByteArray(),
            dest.ToByteArray(),
            data);
    }

    public IPPacket(byte[] data)
    {
        Bytes = data;
    }

    public enum IPProtocol
    {
        Debug = 254,
        UDP = 17
    }

    public IPProtocol Protocol => _packer.GetField(Bytes, 6, b => (IPProtocol)b[0]);

    public byte[] PayLoad => _packer.GetField(Bytes, 10, b => b);
}

public class IPAddress
{
    private byte[] _bytes = new byte[4];

    public IPAddress(string stringRep)
    {
        var parts = stringRep.Split(".");

        for (int i = 0; i < parts.Length; i++)
        {
            var b = byte.Parse(parts[i]);
            _bytes[i] = b;
        }
    }

    public IPAddress(byte[] bytes)
    {
        Array.Copy(bytes, _bytes, 4);
    }

    public const int LENGTH = 4;

    internal byte[] ToByteArray()
    {
        return _bytes;
    }

    public override string ToString()
    {
        return string.Join('.', _bytes.Select(b => b.ToString()));
    }

    public static bool operator ==(IPAddress b1, IPAddress b2) => b1.Equals(b2);
    public static bool operator !=(IPAddress b1, IPAddress b2) => !b1.Equals(b2);

    public override bool Equals(object? obj)
    {
        return obj is IPAddress other && Enumerable.SequenceEqual(other._bytes, _bytes);
    }
    public override int GetHashCode()
    {
        return _bytes.Aggregate(0, (a, b) => HashCode.Combine(a, b.GetHashCode()));
    }
}
