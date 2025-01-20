using Lansweeper.Heijden.Dns.Enums;

namespace Lansweeper.Heijden.Dns;

public class Request
{
    public Header Header { get; } = new()
    {
        OPCODE = OPCode.Query,
        QDCOUNT = 0
    };

    private List<Question> Questions { get; } = [];

    public void AddQuestion(Question question)
    {
        Questions.Add(question);
    }

    public byte[] GetData()
    {
        var data = new List<byte>();
        Header.QDCOUNT = (ushort)Questions.Count;
        data.AddRange(Header.GetData());
        foreach (var q in Questions)
        {
            data.AddRange(q.GetData());
        }
        return data.ToArray();
    }
}