namespace Neo4jNdpClient
{
    public class Packed
    {
        public PackType PackType { get; set; }
        public byte[] Marker { get; set; }
        public byte[] Size { get; set; }
        public byte[] Content { get; set; }
        public byte[] Original { get; }

        public Packed() { }

        public Packed(byte[] original)
        {
            Original = original;
        }
    }
}