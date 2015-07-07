using System.Security.Cryptography.X509Certificates;

namespace Neo4jNdpClient
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class Response<T>
    {
        private IList<Chunk> _chunks;
        public T Content { get; private set; }

        public IList<Chunk> Chunks
        {
            get { return _chunks ?? (_chunks = new List<Chunk>()); }
            set { _chunks = value; }
        }

        public bool AllChunksReceived
        {
            get { return Chunks.Any(c => c.IsFinalChunk); }
        }

        public int Signature
        {
            get
            {
                if (!AllChunksReceived)
                    throw new InvalidOperationException("Whole response not received.");

                return Chunks.First().Signature;
            }
        }


        public Response(byte[] response)
        {
            Chunks = SplitIntoChunks(response);

//            Chunks = new List<Chunk> { new Chunk(response) };
        }

        private IList<Chunk> SplitIntoChunks(byte[] input)
        {
            List<Chunk> output = new List<Chunk>();
            //Zero marker splits... So - let's read until we hit zero...
            List<byte> currentChunk = new List<byte>();
            bool firstZero = false;
            for (int i = 0; i < input.Length; i++)
            {
                currentChunk.Add(input[i]);
                if (input[i] == 0x00)
                {
                    if (firstZero)
                    {
                        firstZero = false;
                        output.Add(new Chunk(currentChunk.ToArray()));
                        currentChunk = new List<byte>();
                    }
                    else firstZero = true;
                }
                else firstZero = false;
            }
            if(currentChunk.Count > 0)
                output.Add(new Chunk(currentChunk.ToArray()));

            return output;
        }

        public void AddChunk(byte[] bytes)
        {
            Chunk c = new Chunk(bytes);
            AddChunk(c);
        }

        public void AddChunk(Chunk chunk)
        {
            Chunks.Add(chunk);
        }
    }
}