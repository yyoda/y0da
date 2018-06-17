namespace Net
{
    public interface IConnection
    {
        bool IsConnected { get; }
        bool Connect(string address, int port);
        bool Disconnect();
    }
}
