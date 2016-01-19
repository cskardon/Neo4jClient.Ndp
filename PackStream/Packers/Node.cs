namespace Neo4jBoltClient
{
    using System.Collections.Generic;
    using System.Linq;
    
    public class Node<T> where T : new()
    {
        public Node() { } 
        public Node(byte[] bytes)
        {
            byte[] editedBytes;
            Id = GetId(bytes, out editedBytes);
            Labels = GetLabels(editedBytes, out editedBytes);
            Data = PackStream.Unpack<T>(editedBytes);
        }

        private static IEnumerable<string> GetLabels(byte[] bytes, out byte[] editedBytes)
        {
            //List of strings.
            var length = Packers.List.GetLengthInBytes(bytes, true);

            var labels = Packers.List.Unpack<string>(bytes.Take(length).ToArray());
            editedBytes = bytes.Skip(length).ToArray();


            return labels;
        }

        private static string GetId(byte[] bytes, out byte[] editedBytes)
        {
            var length = Packers.Text.GetExpectedSize(bytes);
            var markerSize = Packers.Text.SizeOfMarkerInBytes(bytes);

            var bytesToUnpack = bytes.Take(length + markerSize);
            editedBytes = bytes.Skip(length + markerSize).ToArray();
            var id = Packers.Text.Unpack(bytesToUnpack.ToArray());
            return id;
        }


        public string Id { get; set; }
        public IEnumerable<string> Labels { get; set; }
        public T Data { get; set; }
    }

    public class Relationship<T> where T : new()
    {
        public Relationship() { }

        public Relationship(byte[] bytes)
        {
            
        }

        public T Data { get; set; }
    }
}