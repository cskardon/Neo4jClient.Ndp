using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neo4j.Binary.Stream
{
    using System.IO;
    using Neo4jNdpClient;

    public class Neo4jStream : IDisposable
    {
        private Stream Stream { get; }

        public Neo4jStream(Stream stream)
        {
            Stream = stream;
        }

        protected virtual void Dispose(bool isDisposing)
        {
            if (!isDisposing) return;
            Stream?.Dispose();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void Write(byte[] bytes)
        {
            
        }
    }
}