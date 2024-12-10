namespace ConsoleApp.Components;

public class NetworkPort
{
    private NetworkPort? _other;

    private Action<byte[]> _onDataReceived;

    public NetworkPort(Action<byte[]> dataHandler)
    {
        _onDataReceived = dataHandler;
    }

    public void Send(byte[] data)
    {
        Task.Run(() => _other?._onDataReceived?.Invoke(data));
    }

    public void ConnectTo(NetworkPort other)
    {
        _other = other;
        other._other = this;
    }
}