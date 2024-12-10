namespace ConsoleApp.Protocol.MAC;

public class DataPacker
{
    private readonly int[] _format;

    public DataPacker(int[] format)
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
