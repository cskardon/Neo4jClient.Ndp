namespace Neo4j.Binary.Stream
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using Neo4jBoltClient;

    public class Neo4jStream : IDisposable
    {
        private static readonly byte[] ZeroEnding = {0, 0};

        public Neo4jStream(Stream stream)
        {
            Stream = stream;
        }

        private Stream Stream { get; }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool isDisposing)
        {
            if (!isDisposing) return;
            Stream?.Dispose();
        }

        public void Write<T>(Request<T> request)
        {
        }

        public IEnumerable<Struct> Read()
        {
            var chunkSizeInBytes = new byte[2];
            Stream.Read(chunkSizeInBytes, 0, 2);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(chunkSizeInBytes);
            var size = BitConverter.ToInt16(chunkSizeInBytes, 0);

            var data = new List<Struct>();
            while (true)
            {
                byte[] buffer = new byte[size];
                Stream.Read(buffer, 0, size);

                yield return new Struct(buffer);

                Stream.Read(chunkSizeInBytes, 0, 2);
                if (BitConverter.IsLittleEndian)
                    Array.Reverse(chunkSizeInBytes);
                size = BitConverter.ToInt16(chunkSizeInBytes, 0);
                if (size == 0)
                    yield break;
            }
        }
    }

    public class Response
    {
        public byte[] Raw { get; }
        public SignatureBytes Signature { get; }
        public Response(byte[] raw)
        {
            Raw = raw;
            Signature = (SignatureBytes) raw[1];
        }
    }
}