using System.Text;
using ConsoleApp;
using static ConsoleApp.BytePacker;

namespace Tests;

public class BytePackerTests
{
    [Fact]
    public void BytePacker()
    {
        var packer = new BytePacker(new List<PackerSection>
        {
            new PackerSection<int>("Int1", 4, BitConverter.GetBytes, b => BitConverter.ToInt32(b, 0)),
            new PackerSection<string>("String", 0, Encoding.UTF8.GetBytes, Encoding.UTF8.GetString),
        });

        int num = 5454;
        string str = "test string";
        var strLength = Encoding.UTF8.GetByteCount(str);

        var bytes = packer.CreatePacket((uint)strLength);

        packer.Set(bytes, "Int1", num);
        packer.Set(bytes, "String", str);

        int outNum = packer.Get<int>(bytes, "Int1");
        string outStr = packer.Get<string>(bytes, "String");

        Assert.Equal(num, outNum);
        Assert.Equal(str, outStr);
    }
}
