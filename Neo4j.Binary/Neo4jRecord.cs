namespace Neo4jNdpClient
{
    using System.Collections.Generic;

    public class Neo4jRecord
    {
        public IEnumerable<string> Fields { get; }

        public byte[] Packed { get; set; }

        public dynamic Unpacked => Packer.UnpackRecord(Packed, Fields);
        public Neo4jRecord() { }
        public Neo4jRecord(IEnumerable<string> fields)
        {
            Fields = fields;
        }
    }
}