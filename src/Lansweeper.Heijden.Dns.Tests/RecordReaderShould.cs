using FluentAssertions;
using Heijden.DNS;

namespace Lansweeper.Heijden.Dns.Tests;

[TestFixture, Category("UnitTest"), Parallelizable(ParallelScope.All)]
internal class RecordReaderShould
{
    [Test]
    [TestCase(new byte[] { 1 }, 0, 1)]
    [TestCase(new byte[] { 1, 2, 3 }, 2, 3)]
    [TestCase(new byte[] { 1 }, 1, 0)]
    [TestCase(new byte[] { }, 0, 0)]
    public void ReadByte(byte[] input, int position, byte expected)
    {
        var sut = new RecordReader(input, position);
        sut.ReadByte().Should().Be(expected);
    }

    [Test]
    [TestCase(new byte[] { 65 }, 'A')]
    [TestCase(new byte[] { 66 }, 'B')]
    public void ReadChar(byte[] input, char expected)
    {
        var sut = new RecordReader(input);
        sut.ReadChar().Should().Be(expected);
    }
    
    [Test]
    [TestCase(new byte[] { 0, 0 }, (ushort)0)]
    [TestCase(new byte[] { 1, 2 }, (ushort)258)]
    [TestCase(new byte[] { 1 }, (ushort)256)]
    public void ReadUInt16(byte[] input, ushort expected)
    {
        var sut = new RecordReader(input);
        sut.ReadUInt16().Should().Be(expected);
    }
    
    [Test]
    [TestCase(new byte[] { 0, 0, 0, 0 }, (uint)0)]
    [TestCase(new byte[] { 1, 2, 3, 4 }, (uint)16909060)]
    public void ReadUInt32(byte[] input, uint expected)
    {
        var sut = new RecordReader(input);
        sut.ReadUInt32().Should().Be(expected);
    }
    
    [Test]
    [TestCase(new byte[] { 3, 65, 66, 67 }, "ABC")]
    public void ReadString(byte[] input, string expected)
    {
        var sut = new RecordReader(input);
        sut.ReadString().Should().Be(expected);
    }
    
    [Test]
    [TestCase(new byte[] { 0,1,2,3,4,5,6,7,8,9 }, 0, 3, new byte[] { 0,1,2 }, 3)]
    [TestCase(new byte[] { 0,1,2,3,4,5,6,7,8,9 }, 3, 3, new byte[] { 3,4,5 }, 6)]
    [TestCase(new byte[] { 0,1,2,3,4,5,6,7,8,9 }, 8, 5, new byte[] {8,9,0,0,0 }, 10)]
    public void ReadBytes(byte[] input, int position, int readBytes, byte[] expected, int expectedPosition)
    {
        var sut = new RecordReader(input, position);
        sut.ReadBytes(readBytes).Should().BeEquivalentTo(expected);
        sut.Position.Should().Be(expectedPosition);
    }
    
    [Test]
    [TestCase(new byte[] { 0,1,2,3,4,5,6,7,8,9 }, 0, 3, new byte[] { 0,1,2 }, 3)]
    [TestCase(new byte[] { 0,1,2,3,4,5,6,7,8,9 }, 3, 3, new byte[] { 3,4,5 }, 6)]
    [TestCase(new byte[] { 0,1,2,3,4,5,6,7,8,9 }, 8, 5, new byte[] {8,9,0,0,0 }, 10)]
    public void ReadSpan(byte[] input, int position, int readBytes, byte[] expected, int expectedPosition)
    {
        var sut = new RecordReader(input, position);
        sut.ReadSpan(readBytes).ToArray().Should().BeEquivalentTo(expected);
        sut.Position.Should().Be(expectedPosition);
    }


}