namespace Neo4jNdpClient
{
    using System.Collections.Generic;

    public class Neo4jRecord
    {
        private byte[] _packed;
        public IEnumerable<string> Fields { get; }

        public byte[] Packed
        {
            get { return _packed; }
            set
            {
                _packed = value;
                Unpacked = Packer.UnpackRecord(value, Fields);
            }
        }

        public dynamic Unpacked { get; private set; }
        public Neo4jRecord() { }
        public Neo4jRecord(IEnumerable<string> fields)
        {
            Fields = fields;
        }
    }
}