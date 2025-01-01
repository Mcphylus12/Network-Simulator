using ConsoleApp.Protocol.MAC;

namespace ConsoleApp.Protocol;

internal class UDPPacket
{
    private static readonly DataPacker _packer = new DataPacker(
    [
        2,
        2,
        2,
        2,
        0
    ]);

    public byte[] Bytes { get; private set; }
    public ushort TargetPort => _packer.GetField(Bytes, 1, b => BitConverter.ToUInt16(b, 0));
    public byte[] Payload => _packer.GetField(Bytes, 4, b => b);

    public UDPPacket(ushort sourcePort, ushort targetPort, byte[] data)
    {
        byte[] parts = BitConverter.GetBytes(sourcePort);
        byte[] bytes = BitConverter.GetBytes(targetPort);
        Bytes = _packer.Pack(
                parts,
                bytes,
                BitConverter.GetBytes((ushort)(8 + data.Length)),
                [0, 0],
                data
            );
    }

    public UDPPacket(byte[] data)
    {
        Bytes = data;
    }
}
