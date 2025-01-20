using AutoFixture;
using FluentAssertions;
using Heijden.DNS;

namespace Lansweeper.Heijden.Dns.Tests;

[TestFixture, Category("UnitTest"), Parallelizable(ParallelScope.All)]

internal class QuestionShould
{
    [Test]
    public void ParseAndReturnInput()
    {
        var fixture = new Fixture();
        var question = fixture.Create<Question>();
        question.QName = "SomeName.With.Dots";
        var bytes = question.GetData();

        var rr = new RecordReader(bytes);
        var question2 = new Question(rr);
        question2.Should().BeEquivalentTo(question);
    }
}