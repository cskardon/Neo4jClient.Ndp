namespace Neo4jBoltClient
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using Neo4j.Binary;
    using Sockets.Plugin;
    using Sockets.Plugin.Abstractions;

    public static class Bolt
    {
        private static IBitConverter BitConverter => new BigEndianTargetBitConverter();
        private static IChunker Chunker => new Chunker();

        public static class V1
        {
            public static async Task<bool> InitAsync(ITcpSocketClient client, string userAgent)
            {
                var request = new Request<string>(SignatureBytes.Init, userAgent);
                var bytes = await Chunker.ChunkAsync(request);
                await client.WriteStream.WriteAndFlushAsync(bytes);
                
                bytes = new byte[1024];
                var received = await client.ReadStream.ReadAsync(bytes);
                var response = new Response<string>(bytes.Take(received).ToArray());
                return response.Signature == (int) SignatureBytes.Success;
            }

            public static async Task<bool> DoHandShakeAsync(ITcpSocketClient client)
            {
                var bytes = PackVersions(new[] {1, 0, 0, 0});

                await client.WriteStream.WriteAndFlushAsync(bytes);

                bytes = new byte[4];
                await client.ReadStream.ReadAsync(bytes);

                return BitConverter.ToInt32(bytes) == 1;
            }

            private static byte[] PackVersions(IEnumerable<int> versions)
            {
                //This is a 'magic' handshake identifier to indicate we're using 'BOLT' ('GOGOBOLT')
                var aLittleBitOfMagic = BitConverter.GetBytes(0x6060B017);

                var bytes = new List<byte>(aLittleBitOfMagic);
                foreach (var version in versions)
                    bytes.AddRange(BitConverter.GetBytes(version));

                return bytes.ToArray();
            }
        }
    }

    public interface IChunker
    {
        Task<byte[]> ChunkAsync<T>(Request<T> request);
    }

    internal class Chunker : IChunker
    {
        public Task<byte[]> ChunkAsync<T>(Request<T> request)
        {
            throw new NotImplementedException();
        }
    }

    public static class StreamExtensions
    {
        public static async Task WriteAndFlushAsync(this Stream stream, byte[] bytes)
        {
            await stream.WriteAsync(bytes);
            await stream.FlushAsync();
        }

        public static async Task WriteAsync(this Stream stream, byte[] bytes)
        {
            await stream.WriteAsync(bytes, 0, bytes.Length);
        }

        public static async Task<int> ReadAsync(this Stream stream, byte[] bytes)
        {
            return await stream.ReadAsync(bytes, 0, bytes.Length);
        }
    }

    public interface ILogger
    {
        Task DebugAsync(string message);
        Task InfoAsync(string message);
        Task ErrorAsync(string message);
    }

    public class Neo4jBoltClient : IDisposable
    {
        private const string UserAgent = "Neo4jBoltNetClient/0.1";

//        private TcpClient 
//        private readonly EndPoint _endPoint;
//        private readonly Socket _socket;
//
//        public Neo4jBoltClient(Uri uri)
//        {
//            _endPoint = new DnsEndPoint(uri.Host, uri.Port);
//            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
//
//            var tcpClient = new TcpClient(uri.Host, uri.Port);
//            var stream = tcpClient.GetStream();
//            var bitConverter = new BigEndianBitConverter();
//
//            var handshakeOk = PerformHandshake(stream, bitConverter);
//        }
//
////        private bool Init()
////        {
////            var content = GenerateInitMessage(UserAgent);
////            Console.WriteLine(ToString(content, content.Length));
////            _socket.Send(content);
////
////            byte[] data = new byte[1024];
////            int received = _socket.Receive(data, 1024, SocketFlags.None);
////
////            var response = new Response<string>(data.Take(received).ToArray());
////
////            return response.Signature == 0x70;
////        }
//
//
//        private static byte[] PullAllChunk => new byte[] {0x00, 0x02, 0xB0, 0x3F, 0x00, 0x00};
//
//        public void Dispose()
//        {
//            Dispose(true);
//            GC.SuppressFinalize(this);
//        }
//
//        public static string ToString(byte[] bytes, int? numberToRead = null)
//        {
//            if (numberToRead == null)
//                numberToRead = bytes.Length;
//
//            var output = new StringBuilder();
//            for (var i = 0; i < numberToRead; i++)
//                output.AppendFormat("{0},", bytes[i]);
//            return output.ToString();
//        }
//
//        public void Connect()
//        {
//            _socket.Connect(_endPoint);
//            if (!PerformHandshake())
//                throw new InvalidOperationException(
//                    "Handshake could not be performed. Server did not reply as expected.");
//            if (!InitWithStream())
//                throw new InvalidOperationException(
//                    "Initialization could not be performed. Server did not reply as expected.");
//        }
//
//        private bool PerformHandshake(NetworkStream stream, IBitConverter bitConverter)
//        {
//            int[] version = {1, 0, 0, 0};
//            var bytes = new List<byte>();
//            foreach (var i in version)
//            {
//                bytes.AddRange(bitConverter.GetBytes(i));
//            }
//            stream.Write(bytes.ToArray(), 0, bytes.Count);
//            byte[] recBytes = new byte[4];
//            stream.Read(recBytes, 0, recBytes.Length);
//
//            int response = bitConverter.ToInt32(recBytes);
//            return response != 0;
//        }
//
//        private bool PerformHandshake()
//        {
//            _socket.Send(BinaryConverter.IntToBinaryBytes("0x1000"));
//            var recBytes = new byte[4];
//            _socket.Receive(recBytes, 4, SocketFlags.None);
//
//            if (BitConverter.IsLittleEndian)
//                Array.Reverse(recBytes);
//
//            var serverResponse = BitConverter.ToInt32(recBytes, 0);
//            if (serverResponse == 0)
//                return false;
//
//            return true;
//        }
//
//        private static byte[] GenerateInitMessage(string userAgent)
//        {
//            var request = new Request<string>(SignatureBytes.Init, userAgent);
//            return request.GetChunks().First();
//        }
//
//        private bool InitWithStream()
//        {
//            var networkStream = new NetworkStream(_socket);
//            var initMessage = GenerateInitMessage(UserAgent);
//            networkStream.Write(initMessage, 0, initMessage.Length);
//            networkStream.Flush();
//            var buffer = new byte[1024];
//            var read = networkStream.Read(buffer, 0, 1024);
//
//            var r = new Response<string>(buffer.Take(read).ToArray());
//            return r.Signature == (int) SignatureBytes.Success;
//        }
//
//        public IEnumerable<Neo4jRecord> ExecuteCypher2(string cypher, dynamic parameters = null)
//        {
//            var request = new Request<Query>(SignatureBytes.Run, new Query { Cypher = cypher, Parameters = parameters });
//            var chunks = request.GetChunks().First();
//
//            var allChunk = new List<byte>(chunks);
//            allChunk.AddRange(PullAllChunk);
//            _socket.Send(allChunk.ToArray());
//
//
//            var n4js = new Neo4jStream(new NetworkStream(_socket));
//            var readIn = n4js.Read().Single();
//            if (readIn.SignatureByte == SignatureBytes.Success)
//            {
//                var data = n4js.Read();
//                var records = new List<Neo4jRecord>();
//                foreach (var d in data)
//                {
//                    records.Add(d.GetRecord());
//                    int i = 0;
//                }
//
//                return records;
//            }
//
//            
////            var buffer = new byte[1024];
////            var read = _socket.Receive(buffer, 0, 1024, SocketFlags.None);
////            var r = new Response<string>(buffer.Take(read).ToArray());
////            while (!r.AllChunksReceived)
////            {
////                read = _socket.Receive(buffer, 0, 1024, SocketFlags.None);
////                r.AddChunk(buffer.Take(read).ToArray());
////                if (read == 0)
////                    break;
////            }
////
////            if (!r.AllChunksReceived)
////                throw new InvalidOperationException("Missing ending.");
//
//            return null;
//
////            var structs = GetStructs(r.Chunks);
////
////            if (structs[0].SignatureByte != SignatureBytes.Success)
////            {
////                Console.WriteLine("FAIL!!!!");
////                return null;
////            }
////
////            var fieldData = GetMetaData<Dictionary<string, IEnumerable<string>>>(structs[0]);
////            if (!fieldData["fields"].Any())
////            {
////                return null;
////            }
////
////
////            var records = new List<Neo4jRecord>();
////            for (var i = 1; i < structs.Count; i++)
////            {
////                var s = structs[i];
////                if (s.SignatureByte == SignatureBytes.Record)
////                {
////                    var record = new Neo4jRecord(fieldData["fields"]) { Packed = s.ContentWithoutStructAndSignature };
////                    records.Add(record);
////                }
////                if (s.SignatureByte == SignatureBytes.Success)
////                {
////
////                }
////            }
////            return records;
//        }
//
//
//
//        public IEnumerable<Neo4jRecord> ExecuteCypher(string cypher, dynamic parameters = null)
//        {
//            var request = new Request<Query>(SignatureBytes.Run, new Query {Cypher = cypher, Parameters = parameters});
//            var chunks = request.GetChunks().First();
//            
//            var allChunk = new List<byte>(chunks);
//            allChunk.AddRange(PullAllChunk);
//            _socket.Send(allChunk.ToArray());
//
//            var buffer = new byte[1024];
//            var read = _socket.Receive(buffer, 0, 1024, SocketFlags.None);
//            var r = new Response<string>(buffer.Take(read).ToArray());
//            while (!r.AllChunksReceived)
//            {
//                read = _socket.Receive(buffer, 0, 1024, SocketFlags.None);
//                r.AddChunk(buffer.Take(read).ToArray());
//                if (read == 0)
//                    break;
//            }
//
//            if(!r.AllChunksReceived)
//                throw new InvalidOperationException("Missing ending.");
//
//            var structs = GetStructs(r.Chunks);
//
//            if (structs[0].SignatureByte != SignatureBytes.Success)
//            {
//                Console.WriteLine("FAIL!!!!");
//                return null;
//            }
//
//            var fieldData = GetMetaData<Dictionary<string, IEnumerable<string>>>(structs[0]);
//            if (!fieldData["fields"].Any())
//            {
//                return null;
//            }
//
//
//            var records = new List<Neo4jRecord>();
//            for (var i = 1; i < structs.Count; i++)
//            {
//                var s = structs[i];
//                if (s.SignatureByte == SignatureBytes.Record)
//                {
//                    var record = new Neo4jRecord(fieldData["fields"]) {Packed = s.ContentWithoutStructAndSignature};
//                    records.Add(record);
//                }
//                if (s.SignatureByte == SignatureBytes.Success)
//                {
//
//                }
//            }
//            return records;
//        }
//
//        private static string DynamicToString(dynamic d, int numberOfTabs = 0)
//        {
//            if (!(d is ExpandoObject))
//                return "Unhandled type";
//
//            var tabs = new string('\t', numberOfTabs);
//            var dict = (IDictionary<string, object>) d;
//            var output = new StringBuilder();
//            foreach (var key in dict.Keys)
//                output.AppendLine($"{tabs}{key} --> {dict[key]}");
//
//            return output.ToString();
//        }
//
//        public T GetMetaData<T>(Struct s) where T : class, new()
//        {
//            if (s.NumberOfFields == 0)
//                return null;
//
//            //Get actual data
//            var response = PackStream.Unpack<T>(s.ContentWithoutStructAndSignature);
//            return response;
//        }
//
//        public IList<Struct> GetStructs(IList<ChunkAsync> chunks)
//        {
//            return chunks.Select(chunk => Packers.Struct.Unpack(chunk.Content)).ToList();
//        }

        private readonly ITcpSocketClient _client;
        private readonly bool _isSecure;
        private readonly ILogger _logger;
        private readonly Uri _uri;

        public Neo4jBoltClient(Uri uri, ILogger logger, bool isSecure = false, ITcpSocketClient client = null)
        {
            if (uri == null)
                throw new ArgumentNullException(nameof(uri));

            if (_client == null)
                _client = new TcpSocketClient();

            _uri = uri;
            _isSecure = isSecure;
            _logger = logger;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public async Task ConnectAsync()
        {
            await _client.ConnectAsync(_uri.Host, _uri.Port, _isSecure);
            bool handshakeSuccess = await Bolt.V1.DoHandShakeAsync(_client);
            if (handshakeSuccess)
                await Bolt.V1.InitAsync(_client, UserAgent);
        }


        protected virtual void Dispose(bool isDisposing)
        {
            if (!isDisposing)
                return;

            _client?.DisconnectAsync().Wait();
        }
    }
}