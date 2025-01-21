using System.Net;
using AutoFixture;
using FluentAssertions;

namespace Lansweeper.Heijden.Dns.Tests;

[TestFixture, Category("UnitTest"), Parallelizable(ParallelScope.All)]
internal class ResponseShould
{
    [Test]
    [TestCase(1, 16, 0, 8, "042FgAABABAAAAAIAmxzBWxvY2FsAAD/AAHADAABAAEAAAJYAAQKgGECwAwAAQABAAACWAAECgwBAsAMAAEAAQAAAlgABAoKEwPADAABAAEAAAJYAAQKgCECwAwAAQABAAACWAAECgoTAsAMAAEAAQAAAlgABAqAAQLADAACAAEAAA4QAAsIcHJkd2RjMDTADMAMAAIAAQAADhAACwhQUkRXREMwMsAMwAwAAgABAAAOEAALCHByZHdkYzAxwAzADAACAAEAAA4QAALAtMAMAAIAAQAADhAACwhwcmR3ZGMwM8AMwAwAAgABAAAOEAALCHByZHdkYzA1wAzADAACAAEAAA4QAAsIcHJkd2RjMDbADMAMAAIAAQAADhAAAsCdwAwABgABAAAOEAAjwNkKaG9zdG1hc3RlcsAMAAipgwAAA4QAAAJYAAFRgAAADhDADAAcAAEAAAJYABD94VO66aDeETj6Uh6eMJh1wIYAAQABAAAOEAAECoABAsCdAAEAAQAADhAABAoKEwLAtAABAAEAAA4QAAQKChMDwMsAAQABAAAOEAAECgoTA8DZAAEAAQAADhAABAoMAQLA8AABAAEAAA4QAAQKgCECwPAAHAABAAAOEAAQ/eFTuumg3hE4+lIenjCYdcEHAAEAAQAADhAABAqAYQI=")]
    [TestCase(1, 4, 0, 0, "7dyEAAABAAQAAAAACV9zZXJ2aWNlcwdfZG5zLXNkBF91ZHAFbG9jYWwAAAwAAcAMAAwAAQAAAAoAGBBfc3BvdGlmeS1jb25uZWN0BF90Y3DAI8AMAAwAAQAAAAoACwhfYWlycGxhecBLwAwADAABAAAACgAIBV9yYW9wwEvADAAMAAEAAAAKAAkGX3Nvbm9zwEs=")]
    [TestCase(1, 1, 0, 4, "WA+EAAABAAEAAAAECF9haXJwbGF5BF90Y3AFbG9jYWwAAAwAAcAMAAwAAQAAAAoADQpTb25vcyBNb3ZlwAzAMQAhAAEAAAAKABsAAAAAG1gSU29ub3MtNDhBNkI4RUExNTNFwBrAMQAQAAEAAAAKATIFYWNsPTAaZGV2aWNlaWQ9NDg6QTY6Qjg6RUE6MTU6M0UbZmVhdHVyZXM9MHg0NDVGOEEwMCwweDFDMzQwB3JzZj0weDARZnY9cDIwLjgyLjMtNjAxNjAJZmxhZ3M9MHg0Cm1vZGVsPU1vdmUSbWFudWZhY3R1cmVyPVNvbm9zIHNlcmlhbE51bWJlcj00OC1BNi1COC1FQS0xNS0zRTo4DXByb3RvdmVycz0xLjENc3JjdmVycz0zNjYuMBRwaT00ODpBNjpCODpFQToxNTozRRVnaWQ9NDg6QTY6Qjg6RUE6MTU6M0UGZ2NnbD0wQ3BrPThhNzg0Y2VlOTBjOWViMWEwM2Q5NDE5Y2Y1YWQ4YzZmMzZiMGU3ZjI1ZTYzZDQ4N2RiOGEyYTMwNzVmZDlmODnAUAABAAEAAAAKAATAqG9awFAAHAABAAAACgAQ/oAAAAAAAABKprj//uoVPg==")]
    public void ParseInput(int expectedQuestions, int expectedAnswers, int expectedAuthorities, int expectedAdditionals, string base64)
    {
        var bytes = Convert.FromBase64String(base64);

        var response = new Response(new IPEndPoint(IPAddress.Loopback, 1), bytes);
        response.Header.QuestionCount.Should().Be((ushort)expectedQuestions);
        response.Questions.Should().HaveCount(expectedQuestions);
        response.Header.AnswerCount.Should().Be((ushort)expectedAnswers);
        response.Answers.Should().HaveCount(expectedAnswers);
        response.Header.NameServerCount.Should().Be((ushort)expectedAuthorities);
        response.Authorities.Should().HaveCount(expectedAuthorities);
        response.Header.AdditionRecordCount.Should().Be((ushort)expectedAdditionals);
        response.Additionals.Should().HaveCount(expectedAdditionals);
    }
}