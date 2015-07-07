using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Neo4jNdpClient
{
    public class Neo4jNdpClient : IDisposable
    {
        private const string UserAgent = "Neo4jNdpNetClient/0.1";
        private readonly EndPoint _endPoint;
        private readonly Socket _socket;

        public Neo4jNdpClient(Uri uri)
        {
            _endPoint = new DnsEndPoint(uri.Host, uri.Port);
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public static string ToString(byte[] bytes, int? numberToRead = null)
        {
            if (numberToRead == null)
                numberToRead = bytes.Length;

            var output = new StringBuilder();
            for (var i = 0; i < numberToRead; i++)
                output.AppendFormat("{0},", bytes[i]);
            return output.ToString();
        }

        public void Connect()
        {
            _socket.Connect(_endPoint);
            if (!PerformHandshake())
                throw new InvalidOperationException(
                    "Handshake could not be performed. Server did not reply as expected.");
            if (!InitWithStream())
                throw new InvalidOperationException(
                    "Initialization could not be performed. Server did not reply as expected.");
        }

        private bool PerformHandshake()
        {
            _socket.Send(BinaryConverter.IntToBinaryBytes("0x1000"));
            var recBytes = new byte[4];
            _socket.Receive(recBytes, 4, SocketFlags.None);

            if (BitConverter.IsLittleEndian)
                Array.Reverse(recBytes);

            var serverResponse = BitConverter.ToInt32(recBytes, 0);
            if (serverResponse == 0)
                return false;

            return true;
        }

        private static byte[] GenerateInitMessage(string userAgent)
        {
            var request = new Request<string>(SignatureBytes.Init, userAgent);
            return request.GetChunks().First();
        }

        private bool InitWithStream()
        {
            var networkStream = new NetworkStream(_socket);
            var initMessage = GenerateInitMessage(UserAgent);
            networkStream.Write(initMessage, 0, initMessage.Length);
            networkStream.Flush();
            var buffer = new byte[1024];
            var read = networkStream.Read(buffer, 0, 1024);

            var r = new Response<string>(buffer.Take(read).ToArray());
            return r.Signature == (int) SignatureBytes.Success;
        }

//        private bool Init()
//        {
//            var content = GenerateInitMessage(UserAgent);
//            Console.WriteLine(ToString(content, content.Length));
//            _socket.Send(content);
//
//            byte[] data = new byte[1024];
//            int received = _socket.Receive(data, 1024, SocketFlags.None);
//
//            var response = new Response<string>(data.Take(received).ToArray());
//
//            return response.Signature == 0x70;
//        }

        

        private static byte[] PullAllChunk => new byte[] {0x00, 0x02, 0xB0, 0x3F, 0x00, 0x00};


        public IEnumerable<Neo4jRecord> ExecuteCypher(string cypher, dynamic parameters = null)
        {
            var request = new Request<Query>(SignatureBytes.Run, new Query {Cypher = cypher, Parameters = parameters});
            var chunks = request.GetChunks().First();
            //Console.WriteLine(ToString(chunks, chunks.Length));
            

            var allChunk = new List<byte>(chunks);
            allChunk.AddRange(PullAllChunk);
//            Console.WriteLine(ToString(allChunk.ToArray(), allChunk.Count));
            _socket.Send(allChunk.ToArray());

            var buffer = new byte[1024];
            var read = _socket.Receive(buffer, 0, 1024, SocketFlags.Partial);
            var r = new Response<string>(buffer.Take(read).ToArray());
//            Console.WriteLine(ToString(r.Chunks[0].Content));
            var structs = GetStructs(r.Chunks);

            if (structs[0].SignatureByte != SignatureBytes.Success)
            {
                Console.WriteLine("FAIL!!!!");
                return null;
            }

            var fieldData = GetMetaData<Dictionary<string, IEnumerable<string>>>(structs[0]);
            if (!fieldData["fields"].Any())
            {
//                Console.WriteLine("No results to read.");
                return null;
            }


            var records = new List<Neo4jRecord>();
            for (int i = 1; i < structs.Count; i++)
            {
                var s = structs[i];
                if (s.SignatureByte == SignatureBytes.Record)
                {
                    var record = new Neo4jRecord(fieldData["fields"]) {Packed = s.ContentWithoutStructAndSignature };
                    records.Add(record);
                }
                if (s.SignatureByte == SignatureBytes.Success)
                {
//                    Console.WriteLine("Finished reading from N4j");
                    //End of records
                }
            }
            return records;
//            Console.WriteLine("Results");
//            foreach (var record in records)
//            {
//                Console.WriteLine("RECORD:");
//                Console.WriteLine(DynamicToString(record.Unpacked, 1));
//            }
        }

        private static string DynamicToString(dynamic d, int numberOfTabs = 0)
        {
            if (!(d is ExpandoObject))
                return "Unhandled type";

            var tabs = new string('\t', numberOfTabs);
            var dict = (IDictionary<string, object>) d;
            var output = new StringBuilder();
            foreach (var key in dict.Keys)
                output.AppendLine($"{tabs}{key} --> {dict[key]}");

            return output.ToString();
        }

        

        public T GetMetaData<T>(Neo4jStruct s) where T: class,new()
        {
            if(s.NumberOfFields == 0)
                return null;

            //Get actual data
            var response = Packer.Unpack<T>(s.ContentWithoutStructAndSignature);
            return (T) response;
        }

        public IList<Neo4jStruct> GetStructs(IList<Chunk> chunks)
        {
            return chunks.Select(chunk => Packers.Struct.Unpack(chunk.Content)).ToList();
        }

        protected virtual void Dispose(bool isDisposing)
        {
            if (!isDisposing)
                return;

            _socket.Shutdown(SocketShutdown.Both);
            _socket.Close();
        }
    }
}