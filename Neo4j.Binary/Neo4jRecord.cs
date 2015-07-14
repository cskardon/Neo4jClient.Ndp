namespace Neo4jNdpClient
{
    using System.Collections.Generic;
    using System.Linq;

    public class Neo4jRecord
    {
        public IEnumerable<string> Fields { get; }

        public byte[] Packed { get; set; }

        public dynamic Unpacked => Packer.UnpackRecord(Packed, Fields);

        public IEnumerable<Node<T>> As<T>() where T : new()
        {
            var structs = Packers.List.Unpack<Neo4jStruct>(Packed);
            return structs.Select(neo4JStruct => neo4JStruct.GetNode<T>()).ToList();
        }

        public Neo4jRecord() { }
        public Neo4jRecord(IEnumerable<string> fields)
        {
            Fields = fields;
        }
    }

    
}