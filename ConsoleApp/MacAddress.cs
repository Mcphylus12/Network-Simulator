namespace Tests;

public class MacAddress
{
    private byte[] _data = new byte[6];
    public const int LENGTH = 6;
    public bool IsBroadcast => _data.All(b => b == 255);

    public readonly static MacAddress BroadCastAddress = new MacAddress([255, 255, 255, 255, 255, 255]);
    public readonly static MacAddress EmptyAddress = new MacAddress([0, 0, 0, 0, 0, 0]);

    public MacAddress() => Random.Shared.NextBytes(_data);

    public MacAddress(byte[] bytes)
    {
        if (bytes.Length != 6) throw new NotSupportedException("Mac addresses should be length 6");
        Array.Copy(bytes, _data, 6);
    }

    internal byte[] ToByteArray() => _data;
    public override bool Equals(object? obj) => obj is MacAddress other && other._data.SequenceEqual(_data);
    public override int GetHashCode() => _data.Aggregate(0, (a, b) => HashCode.Combine(a, b.GetHashCode()));
    public static bool operator ==(MacAddress b1, MacAddress b2) => b1.Equals(b2);
    public static bool operator !=(MacAddress b1, MacAddress b2) => !b1.Equals(b2);

    public override string ToString()
    {
        return string.Join(':', _data.Select(b => b.ToString()));
    }
}
