namespace Neo4j.Binary.Tests
{
    using System.Linq.Expressions;
    using FluentAssertions;
    using Xunit;

    public class BinaryConverterTests
    {
        public class IntToBinaryBytes_IntMethod
        {
            [Fact]
            public void GeneratesCorrect_ForOne()
            {
                const int input = 1;
                byte[] expected = {0, 0, 0, 1};

                var actual = BinaryConverter.IntToBinaryBytes(input);
                actual.Should().Equal(expected);
            }

            [Fact]
            public void GeneratesCorrect_ForFifteen()
            {
                const int input = 15;
                byte[] expected = {1, 1, 1, 1};

                var actual = BinaryConverter.IntToBinaryBytes(input);
                actual.Should().Equal(expected);
            }
        }

        public class IntToBinaryBytes_StringMethod
        {
            [Fact]
            public void GeneratesCorrect_ForOne()
            {
                const string input = "0x1";
                byte[] expected = {0, 0, 0, 1};

                var actual = BinaryConverter.IntToBinaryBytes(input);
                actual.Should().Equal(expected);
            }

            [Fact]
            public void GeneratesCorrect_ForHandshake()
            {
                const string input = "0x1000";
                byte[] expected = {0,0,0,1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };

                var actual = BinaryConverter.IntToBinaryBytes(input);
                actual.Should().Equal(expected);
            }

            [Theory]
            [InlineData("0x0", new byte[] { 0, 0, 0, 0 })]
            [InlineData("0x1", new byte[] { 0, 0, 0, 1 })]
            [InlineData("0x2", new byte[] { 0, 0, 1, 0 })]
            [InlineData("0x3", new byte[] { 0, 0, 1, 1 })]
            [InlineData("0x4", new byte[] { 0, 1, 0, 0 })]
            [InlineData("0x5", new byte[] { 0, 1, 0, 1 })]
            [InlineData("0x6", new byte[] { 0, 1, 1, 0 })]
            [InlineData("0x7", new byte[] { 0, 1, 1, 1 })]
            [InlineData("0x8", new byte[] { 1, 0, 0, 0 })]
            [InlineData("0x9", new byte[] { 1, 0, 0, 1 })]
            [InlineData("0xA", new byte[] { 1, 0, 1, 0 })]
            [InlineData("0xB", new byte[] { 1, 0, 1, 1 })]
            [InlineData("0xC", new byte[] { 1, 1, 0, 0 })]
            [InlineData("0xD", new byte[] { 1, 1, 0, 1 })]
            [InlineData("0xE", new byte[] { 1, 1, 1, 0 })]
            [InlineData("0xF", new byte[] { 1, 1, 1, 1 })]
            [InlineData("0xB1", new byte[] { 1, 0, 1, 1, 0, 0, 0, 1 })]
            [InlineData("0x01", new byte[] { 0, 0, 0, 0, 0, 0, 0, 1 })]
            [InlineData("0x0F", new byte[] { 0, 0, 0, 0, 1, 1, 1, 1 })]
            [InlineData("0x10", new byte[] { 0, 0, 0, 1, 0, 0, 0, 0 })]
            [InlineData("0x2F", new byte[] { 0, 0, 1, 0, 1, 1, 1, 1 })]
            [InlineData("0x7F", new byte[] { 0, 1, 1, 1, 1, 1, 1, 1 })]
            public void GeneratesCorrectForHexCharacters(string input, byte[] expected)
            {
                var actual = BinaryConverter.IntToBinaryBytes(input);
                actual.Should().Equal(expected, input);
            }
        }

        public class IsHexMethod
        {
            [Theory]
            [InlineData("0x0", true)]
            [InlineData("0x0F", true)]
            [InlineData("0x", false)]
            [InlineData("0F", false)]
            [InlineData("0x0f", true)]
            public void MatchesCorrectly(string input, bool expected)
            {
                var actual = BinaryConverter.IsHex(input);
                actual.Should().Be(expected);
            }
        }
    }
}