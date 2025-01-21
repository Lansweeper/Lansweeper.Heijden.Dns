using AutoFixture;
using FluentAssertions;

namespace Lansweeper.Heijden.Dns.Tests;

[TestFixture, Category("UnitTest"), Parallelizable(ParallelScope.All)]
internal class QuestionShould
{
    [Test]
    public void ParseAndReturnInput()
    {
        var fixture = new Fixture();
        var question = fixture.Create<Question>();
        question.Name = "SomeName.With.Dots";
        var bytes = question.GetData().ToArray();

        var rr = new RecordReader(bytes);
        var question2 = new Question(rr);
        question2.Should().BeEquivalentTo(question);
    }
}