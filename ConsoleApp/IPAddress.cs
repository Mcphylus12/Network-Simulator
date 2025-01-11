namespace ConsoleApp;

public class IPAddress
{
    public const int LENGTH = 4;
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

    public IPAddress(byte[] bytes) => Array.Copy(bytes, _bytes, 4);
    internal byte[] ToByteArray() => _bytes;
    public override string ToString() => string.Join('.', _bytes.Select(b => b.ToString()));
    public static bool operator ==(IPAddress b1, IPAddress b2) => b1.Equals(b2);
    public static bool operator !=(IPAddress b1, IPAddress b2) => !b1.Equals(b2);

    public static implicit operator IPAddress(string stringrep) => new IPAddress(stringrep);
    public override bool Equals(object? obj) => obj is IPAddress other && Enumerable.SequenceEqual(other._bytes, _bytes);
    public override int GetHashCode() => _bytes.Aggregate(0, (a, b) => HashCode.Combine(a, b.GetHashCode()));
}
