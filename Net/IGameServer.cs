namespace Net
{
    public interface IListener
    {
        bool IsRunning { get; }
        bool Start(int port, int connectionCount);
        void Stop();
    }
}
