namespace Net
{
    public interface IMessageTransporter
    {
        bool Send(byte[] datae);
        bool Receive(ref byte[] buffer);
    }
}
