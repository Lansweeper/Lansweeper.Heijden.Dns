using AutoFixture;
using FluentAssertions;
using Lansweeper.Heijden.Dns.Enums;

namespace Lansweeper.Heijden.Dns.Tests;

[TestFixture, Category("UnitTest"), Parallelizable(ParallelScope.All)]
internal class RequestShould
{
    [Test]
    public void GetCorrectBytes()
    {
        var fixture = new Fixture();
        var question = fixture.Create<Question>();

        var req = fixture.Create<Request>();
        req.Questions.Clear();
        req.AddQuestion(question);

        var bytes = req.GetData().ToArray();

        var rr = new RecordReader(bytes);
        // Skip 4 bytes (ID + Flags) and read QDCOUNT
        var count = rr.ReadUInt16(4); 
        count.Should().Be(1);

        // Skip 6 bytes to the beginning of the question
        rr.Position += 6;
        var question2 = new Question(rr);
        question2.Should().BeEquivalentTo(question);
    }

    [Test]
    public void AddQuestions()
    {
        var req = new Request();
        req.Header.QuestionCount.Should().Be(0);
        req.Questions.Should().BeEmpty();

        req.AddQuestion(new Question("q", QType.ANY, QClass.ANY));
        req.Header.QuestionCount.Should().Be(1);
        req.Questions.Should().ContainSingle();
        
        req.AddQuestion(new Question("q", QType.ANY, QClass.ANY));
        req.Header.QuestionCount.Should().Be(2);
        req.Questions.Should().HaveCount(2);
    }
}