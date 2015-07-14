namespace Neo4jNdpClient
{
    using System;
    using System.Linq;

    public class Neo4jStruct
    {
        public byte[] OriginalBytes { get; }
        private int _numberOfFields;

        public Neo4jStruct(byte[] originalBytes)
        {
            OriginalBytes = originalBytes;
        }

        public int NumberOfFields
        {
            get { return _numberOfFields; }
            set
            {
                if (OriginalBytes == null)
                    throw new InvalidOperationException("No fields can be set without original bytes!");
                if (value < 0)
                    throw new ArgumentOutOfRangeException(nameof(value), value, "Number of fields must be a positive number.");

                _numberOfFields = value;
                if (NumberOfFields <= 15)
                    ContentWithoutStructAndSignature = OriginalBytes.Skip(2).ToArray();
                else if (NumberOfFields >= 16 && NumberOfFields <= 255)
                    ContentWithoutStructAndSignature = OriginalBytes.Skip(3).ToArray();
                else if (NumberOfFields >= 256 && NumberOfFields <= 65535)
                    ContentWithoutStructAndSignature = OriginalBytes.Skip(4).ToArray();
            }
        }

        public byte[] ContentWithoutStructAndSignature { get; private set; }
        public SignatureBytes SignatureByte { get; set; }

        public Node<T> GetNode<T>() where T : new()
        {
            if(SignatureByte != SignatureBytes.Node)
                throw new InvalidOperationException("The data is not a node.");

            return new Node<T>(ContentWithoutStructAndSignature);
        }

        public override string ToString()
        {
            if (OriginalBytes != null && OriginalBytes.Length >= 0)
                return BitConverter.ToString(OriginalBytes);
            return "No original bytes to convert";
        }
    }
}