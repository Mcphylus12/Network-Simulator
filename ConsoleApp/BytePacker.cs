using System.Xml.Linq;

namespace ConsoleApp;

public class NamedBytePacker
{
    private readonly Dictionary<string, (int Offset, int Size)> _dataFormat;
    public int Length { get; private set; }

    public NamedBytePacker(IEnumerable<KeyValuePair<string, int>> format)
    {
        int offset = 0;
        _dataFormat = new Dictionary<string, (int Offset, int Size)>();

        foreach (var item in format)
        {
            _dataFormat[item.Key] = (offset, item.Value);
            offset += item.Value;
        }

        Length = offset;
    }

    internal void Set(byte[] bytes, string name, byte[] data)
    {
        if (!_dataFormat.TryGetValue(name, out var info))
        {
            throw new Exception(name + " is not a support field");
        }

        Array.Copy(data, 0, bytes, info.Offset, info.Size);
    }

    internal byte[] Get(byte[] bytes, string name)
    {
        if (!_dataFormat.TryGetValue(name, out var info))
        {
            throw new Exception(name + " is not a support field");
        }

        if (info.Size == 0)
        {
            return bytes[info.Offset..];
        }
        else
        {
            return bytes[info.Offset..(info.Offset + info.Size)];
        }
    }
}

public class BytePacker
{
    private readonly int[] _format;

    public BytePacker(int[] format)
    {
        _format = format;
    }

    internal byte[] Pack(params byte[][] parts)
    {
        var bytes = new byte[parts.Sum(p => p.Length)];
        using var stream = new MemoryStream(bytes);

        for (int i = 0; i < parts.Length; i++)
        {
            stream.Write(parts[i], 0, _format[i] > 0 ? _format[i] : parts[i].Length);
        }

        return bytes;
    }

    internal T GetField<T>(byte[] bytes, int index, Func<byte[], T> builder)
    {
        var start = _format[0..index].Sum();

        var length = _format[index];

        if (length == 0)
        {
            return builder(bytes[start..]);
        }
        else
        {
            return builder(bytes[start..(start + length)]);
        }
    }
}
