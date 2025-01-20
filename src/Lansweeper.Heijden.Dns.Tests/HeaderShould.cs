using AutoFixture;
using FluentAssertions;

namespace Lansweeper.Heijden.Dns.Tests;

[TestFixture, Category("UnitTest"), Parallelizable(ParallelScope.All)]

internal class HeaderShould
{
    [Test]
    public void ParseAndReturnInput()
    {
        var fixture = new Fixture();
        var header = fixture.Create<Header>();
        var bytes = header.GetData();

        var rr = new RecordReader(bytes);
        var header2 = new Header(rr);
        header2.Should().BeEquivalentTo(header);
    }
}