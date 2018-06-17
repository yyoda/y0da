using System;
using System.Collections.Generic;
using System.IO;

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

    public class PacketQueue
    {
        private MemoryStream _stream = new MemoryStream();
        private LinkedList<PacketMetadata> _metadatas = new LinkedList<PacketMetadata>();
        private object _blocker = new object();
        private int _offset = 0;

        public bool Enqueue(byte[] data)
        {
            var size = data.Length;

            if (data == null)
            {
                return false;
            }

            if (data.Length <= 0)
            {
                return false;
            }

            lock (_blocker)
            {
                _metadatas.AddLast(new PacketMetadata(_offset, size));

                _stream.Position = _offset;
                _stream.Write(data, 0, size);
                _stream.Flush();

                _offset += size;
            }

            return true;
        }

        public bool Dequeue(ref byte[] buffer)
        {
            if (_metadatas.Count <= 0)
            {
                return false;
            }

            lock (_blocker)
            {
                var metadata = _metadatas.First.Value;

                _stream.Position = metadata.Offset;

                var receiveSize = _stream.Read(buffer, 0, metadata.Size);

                if (receiveSize > 0)
                {
                    _metadatas.RemoveFirst();
                }

                if (_metadatas.Count <= 0)
                {
                    Clear();
                }

                return receiveSize > 0;
            }
        }

        public void Clear()
        {
            var buffer = _stream.GetBuffer();

            Array.Clear(buffer, 0, buffer.Length);

            _stream.Position = 0;
            _stream.SetLength(0);

            _offset = 0;
        }
    }
}
