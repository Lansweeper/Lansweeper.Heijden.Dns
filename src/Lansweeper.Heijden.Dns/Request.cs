using Lansweeper.Heijden.Dns.Enums;

namespace Lansweeper.Heijden.Dns;

public class Request
{
    public Header Header { get; } = new()
    {
        OperationCode = OPCode.Query,
        QuestionCount = 0
    };

    internal List<Question> Questions { get; } = [];

    public void AddQuestion(Question question)
    {
        Questions.Add(question);
        Header.QuestionCount = (ushort)Questions.Count;
    }

    public byte[] GetData()
    {
        Header.QuestionCount = (ushort)Questions.Count;

        var data = new List<byte>();
        data.AddRange(Header.GetData());
        foreach (var q in Questions)
        {
            data.AddRange(q.GetData());
        }
        return data.ToArray();
    }
}