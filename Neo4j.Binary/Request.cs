namespace Neo4jNdpClient
{
    using System;
    using System.Collections.Generic;

    public class Request<T>
    {
        /// <summary>Ending for a chunk.</summary>
        private static readonly byte[] ZeroEnding = { 0, 0 };
        public T Content { get; }
        public SignatureBytes Signature { get; }

        public Request(SignatureBytes signature, T content)
        {
            Content = content;
            Signature = signature;
        }

        private static byte GetStructSize()
        {
            if (typeof(T).IsPrimitive || typeof(T) == typeof(string))
                return 0xB1;

            if (typeof (T) == typeof (Query))
                return 0xB2;

            return 0xB0;
            //Reflection here I'm guessing.
            throw new NotImplementedException();
        }

        private static byte[] GenerateChunkHeader(int length)
        {
            return Packer.ConvertSizeToBytes(length, 2);
        }

        public ICollection<byte[]> GetChunks(int chunkSize = 65535, bool includeZeroEnding = true)
        {
            if (chunkSize > 65535)
                throw new ArgumentOutOfRangeException(nameof(chunkSize), chunkSize, "Chunk size too big!");

            var fullMessage = new List<byte>();
            fullMessage.Add(GetStructSize());
            fullMessage.Add((byte)Signature);
            var packed = Packer.Pack(Content);
            fullMessage.AddRange(packed);
            
            var header = GenerateChunkHeader(fullMessage.Count);
            for (int i = header.Length - 1; i >= 0; i--)
                fullMessage.Insert(0, header[i]);

            if (includeZeroEnding)
                fullMessage.AddRange(ZeroEnding);

            return new List<byte[]> { fullMessage.ToArray() };
        }
    }
}