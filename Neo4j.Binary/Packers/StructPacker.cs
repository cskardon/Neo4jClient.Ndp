using System.CodeDom;
using System.Globalization;

namespace Neo4jNdpClient
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    /*
      Structures
        ----------

        Structures represent composite values and consist, beyond the marker, of a
        single byte signature followed by a sequence of fields, each an individual
        value. The size of a structure is measured as the number of fields, not the
        total packed byte size. The markers used to denote a structure are described
        in the table below:

          Marker | Size                                        | Maximum size
         ========|=============================================|=======================
          B0..BF | contained within low-order nibble of marker | 15 fields
          DC     | 8-bit big-endian unsigned integer           | 255 fields
          DD     | 16-bit big-endian unsigned integer          | 65 535 fields

        The signature byte is used to identify the type or class of the structure.
        Signature bytes may hold any value between 0 and +127. Bytes with the high bit
        set are reserved for future expansion.

        For structures containing fewer than 16 fields, the marker byte should
        contain the high-order nibble `1011` followed by a low-order nibble
        containing the size. The marker is immediately followed by the signature byte
        and the field values.

        For structures containing 16 fields or more, the marker 0xDC or 0xDD should
        be used, depending on scale. This marker is followed by the size, the signature
        byte and the actual fields, serialised in order. Examples follow below:

            B3 01 01 02 03  -- Struct(sig=0x01, fields=[1,2,3])

            DC 10 7F 01  02 03 04 05  06 07 08 09  00 01 02 03
            04 05 06  -- Struct(sig=0x7F, fields=[1,2,3,4,5,6,7,8,9,0,1,2,3,4,5,6]
     */



    public interface IPacker
    {
        byte[] Pack<T>(T content);
    }

    public interface IUnpacker
    {
        T Unpack<T>(byte[] content) where T : new();
    }


    public class Neo4jStruct
    {
        private readonly byte[] _originalBytes;
        private int _numberOfFields;

        public int NumberOfFields
        {
            get { return _numberOfFields; }
            set
            {
                if(_originalBytes == null)
                    throw new InvalidOperationException("No fields can be set without original bytes!");
                if(value < 0)
                    throw new ArgumentOutOfRangeException(nameof(value), value, "Number of fields must be a positive number.");

                _numberOfFields = value;
                if (NumberOfFields <= 15)
                    ContentWithoutStructAndSignature = _originalBytes.Skip(2).ToArray();
                else if (NumberOfFields >= 16 && NumberOfFields <= 255)
                    ContentWithoutStructAndSignature = _originalBytes.Skip(3).ToArray();
                else if (NumberOfFields >= 256 && NumberOfFields <= 65535)
                    ContentWithoutStructAndSignature = _originalBytes.Skip(4).ToArray();
            }
        }

        public byte[] ContentWithoutStructAndSignature { get; private set; }

        public SignatureBytes SignatureByte { get; set; }

        public Neo4jStruct(byte[] originalBytes)
        {
            _originalBytes = originalBytes;
        }
        

        public override string ToString()
        {
            if(_originalBytes != null && _originalBytes.Length >=0 )
                return BitConverter.ToString(_originalBytes);
            return "No original bytes to convert";
        }
    }

    public static partial class Packers
    {
        public static class Struct
        {
            private static int GetNumberOfFields(byte[] bytes)
            {
                if (bytes[0] >= 0xB0 && bytes[0] <= 0xBF)
                    return bytes[0] - 0xB0;

                if (bytes[0] == 0xDC)
                    return bytes[1];

                if (bytes[0] == 0xDD)
                {
                    var markerSize = bytes.Skip(1).Take(2).ToArray();
                    return int.Parse(BitConverter.ToString(markerSize).Replace("-", ""), NumberStyles.HexNumber);
                }

                throw new ArgumentOutOfRangeException(nameof(bytes), bytes[0], "Unknown Marker");
            }
            public static bool IsStruct(byte[] content)
            {
                return (content[0] >= 0xB0 && content[0] <= 0xBF) || content[0] == 0xDC || content[0] == 0xDD;
            }

            public static Neo4jStruct Unpack(byte[] content)
            {
                if (!IsStruct(content))
                    throw new ArgumentException("Content doesn't represent a Struct.", nameof(content));

                var output = new Neo4jStruct(content);
                output.NumberOfFields = GetNumberOfFields(content);
                output.SignatureByte = GetSignatureByte(content, output.NumberOfFields);

                return output;
            }

            private static SignatureBytes GetSignatureByte(byte[] content, int numberOfFields)
            {
                int skip = 1;
                if (numberOfFields >= 16 && numberOfFields <= 255)
                    skip = 2;
                if (numberOfFields >= 256 && numberOfFields <= 65535) 
                    skip = 3;

                var signatureBytes = content.Skip(skip).Take(1).Single();
                return (SignatureBytes) signatureBytes;
            }
        }
    }
}