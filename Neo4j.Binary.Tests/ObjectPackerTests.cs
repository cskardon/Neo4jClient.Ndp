using System;
using System.Collections.Generic;
using FluentAssertions;
using Neo4jNdpClient;
using Xunit;

namespace Neo4j.Binary.Tests
{
    public class ObjectPackerTests
    {
        private class SimpleClassWithField
        {
            public bool _a;
        }

        private class SimpleClass
        {
            public bool A { get; set; }
        }

        private class MultiPropertyClass : SimpleClass
        {
            public bool B { get; set; }
        }

        private class SimpleClassWithPrivateProperty : SimpleClass
        {
            private bool B { get; set; }
        }

        private class SimpleClassWithNdpIgnoredProperty : SimpleClass
        {
            [NdpIgnore]
            public bool B { get; set; }
        }

        public class PackMethod
        {
            [Fact]
            public void PacksAnonymousCorrectly()
            {
                var toPack = new {A = true};
                var expected = new List<byte> {0xA1, 0x81, 0x41, (byte) Markers.True}; //1 fields


                var actual = Packers.Map.Pack(toPack);
                actual.Should().Equal(expected);
            }

            [Fact]
            public void PacksDefinedClassCorrectly()
            {
                var toPack = new SimpleClass {A = true};
                var expected = new List<byte> {0xA1, 0x81, 0x41, (byte) Markers.True}; //1 fields


                var actual = Packers.Map.Pack(toPack);
                actual.Should().Equal(expected);
            }

            [Fact]
            public void PacksDefinedClassWithMultiplePropertiesCorrectly()
            {
                var toPack = new MultiPropertyClass {A = true, B = false};
                var expected = new List<byte> {0xA2, 0x81, 0x42, (byte) Markers.False, 0x81, 0x41, (byte) Markers.True};


                var actual = Packers.Map.Pack(toPack);
                actual.Should().Equal(expected);
            }

            [Fact]
            public void PacksDefinedClassCorrectlyIgnoringPrivateProperties()
            {
                var toPack = new SimpleClassWithPrivateProperty {A = true};
                var expected = new List<byte> {0xA1, 0x81, 0x41, (byte) Markers.True}; //1 fields


                var actual = Packers.Map.Pack(toPack);
                actual.Should().Equal(expected);
            }

            [Fact]
            public void PacksDefinedClassCorrectlyIgnoringNdpIgnoredProperties()
            {
                var toPack = new SimpleClassWithNdpIgnoredProperty {A = true};
                var expected = new List<byte> {0xA1, 0x81, 0x41, (byte) Markers.True}; //1 fields


                var actual = Packers.Map.Pack(toPack);
                actual.Should().Equal(expected);
            }

            [Theory]
            [InlineData(0, new byte[] {0xA0})]
            [InlineData(1, new byte[] {0xA1, 0x82, 0x61, 0x30, 0x82, 0x61, 0x30})]
            public void PacksDictionary_StringString_Correctly(int size, byte[] expected)
            {
                var dictionary = new Dictionary<string, string>();
                for (var i = 0; i < size; i++)
                    dictionary.Add("a" + i, "a" + i);


                var actual = Packers.Map.Pack(dictionary);
                actual.Should().Equal(expected);
            }
        }

        public class UnpackMethod
        {
            [Theory]
            [InlineData(0, new byte[] {0xA0})]
            [InlineData(1, new byte[] {0xA1, 0x82, 0x61, 0x30, 0x82, 0x61, 0x30})]
            public void UnpacksDictionary_StringString_Correctly(int size, byte[] input)
            {
                var expected = new Dictionary<string, string>();
                for (var i = 0; i < size; i++)
                    expected.Add("a" + i, "a" + i);

                var actual = Packers.Map.Unpack<Dictionary<string, string>>(input);
                actual.Should().Equal(expected);
            }

            [Fact]
            public void UnpacksDefinedClass_WithProperty()
            {
                var expected = new SimpleClass {A = true};
                var toUnpack = new List<byte> {0xA1, 0x81, 0x41, (byte) Markers.True}; //1 fields


                var actual = Packers.Map.Unpack<SimpleClass>(toUnpack.ToArray());
                actual.ShouldBeEquivalentTo(expected);
            }

            [Fact]
            public void UnpacksDefinedClass_WithField()
            {
                var expected = new SimpleClassWithField {_a = true};
                var toUnpack = new List<byte> {0xA1, 0x82, 0x5F, 0x61, (byte) Markers.True}; //1 fields


                var actual = Packers.Map.Unpack<SimpleClassWithField>(toUnpack.ToArray());
                actual.ShouldBeEquivalentTo(expected);
            }

            [Fact]
            public void UnpPacksDefinedClassCorrectlyIgnoringNdpIgnoredProperties()
            {
                var expected = new SimpleClassWithNdpIgnoredProperty {A = true};
                var toUnpack = new List<byte> {0xA1, 0x81, 0x41, (byte) Markers.True}.ToArray(); //1 fields


                var actual = Packers.Map.Unpack<SimpleClassWithNdpIgnoredProperty>(toUnpack);
                actual.ShouldBeEquivalentTo(expected);
            }

            [Fact]
            public void UnpacksSuccessMetaDataProperly()
            {
                var bytes = new byte[] {0xA1, 0x86, 0x66, 0x69, 0x65, 0x6C, 0x64, 0x73, 0x91, 0x81, 0x78};
                var expected = new Dictionary<string, dynamic> {{"fields", new List<dynamic> {"x"}}};

                var actual = Packers.Map.Unpack<Dictionary<string, dynamic>>(bytes);
                actual.Keys.Should().BeEquivalentTo(expected.Keys);
                Assert.Equal(actual.Values, expected.Values);
            }
            [Fact]
            public void UnpacksSuccessMetaDataProperlyAsString()
            {
                var bytes = new byte[] { 0xA1, 0x86, 0x66, 0x69, 0x65, 0x6C, 0x64, 0x73, 0x91, 0x81, 0x78 };
                var expected = new Dictionary<string, IEnumerable<string>> { { "fields", new List<string> { "x" } } };

                var actual = Packers.Map.Unpack<Dictionary<string, IEnumerable<string>>>(bytes);

                foreach (var kvp in expected)
                {
                    actual.Keys.Should().Contain(kvp.Key);
                    foreach (var val in kvp.Value)
                        actual[kvp.Key].Should().Contain(val);
                }
            }

            [Fact(Skip = "To come back to")]
            public void UnpacksAnonymousCorrectly()
            {
                throw new NotImplementedException();
            }
        }
    }
}