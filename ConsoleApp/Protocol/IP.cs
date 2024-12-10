
using ConsoleApp.Protocol.MAC;

namespace ConsoleApp.Protocol;

internal class IPPacket
{
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
