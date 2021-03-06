namespace Neo4jBoltClient
{
    using System;
    using System.Collections.Generic;

    public class Query
    {
        public string Cypher { get; set; }
        public dynamic Parameters { get; set; }

        public byte[] GetBytes()
        {
            var output = new List<byte>();

            output.AddRange(PackStream.Pack(Cypher));
            if (Parameters == null)
            {
                output.AddRange(new byte[] { 0xA0 });
            }
            else
            {
                throw new NotImplementedException();
            }

            return output.ToArray();
        }
    }
}