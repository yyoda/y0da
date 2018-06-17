namespace Net
{
    public struct PacketMetadata
    {
        public PacketMetadata(int offset, int size)
        {
            Offset = offset;
            Size = size;
        }

        public int Offset { get; }
        public int Size { get; }
    }
}
