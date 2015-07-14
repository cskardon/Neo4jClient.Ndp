namespace Neo4jNdpClient
{
    using System.Linq;

    public class Chunk
    {
        public int Signature { get; }
        public byte[] Header { get; private set; }
        public byte[] Content { get; }
        public bool IsFinalChunk { get; }

        public Chunk(byte[] bytes)
        {
            Header = bytes.Take(2).ToArray();
            IsFinalChunk = EndsWithZero(bytes);

            Content = IsFinalChunk ? bytes.Skip(2).Take(bytes.Length - 4).ToArray() : bytes.Skip(2).ToArray();
            Signature = GetSignature(Content);
        }

        private static int GetSignature(byte[] content)
        {
            int signature = content[1];
            return signature;
        }

        private static bool EndsWithZero(byte[] bytes)
        {
            if (bytes[bytes.Length - 1] == 0x0)
                if (bytes[bytes.Length - 2] == 0x0)
                    return true;
            return false;
        }
    }
}