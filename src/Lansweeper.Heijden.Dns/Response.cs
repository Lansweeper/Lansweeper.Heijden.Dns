using System.Net;
using Lansweeper.Heijden.Dns.Records;

namespace Lansweeper.Heijden.Dns;

public class Response
{
    /// <summary>
    /// List of Question records
    /// </summary>
    public List<Question> Questions { get; set; } = [];

    /// <summary>
    /// List of AnswerRR records
    /// </summary>
    public List<AnswerResourceRecord> Answers { get; set; } = [];

    /// <summary>
    /// List of AuthorityRR records
    /// </summary>
    public List<AuthorityResourceRecord> Authorities { get; set; } = [];

    /// <summary>
    /// List of AdditionalRR records
    /// </summary>
    public List<AdditionalResourceRecord> Additionals { get; set; } = [];

    public Header Header { get; set; }

    /// <summary>
    /// Error message, empty when no error
    /// </summary>
    public string Error { get; set; } = string.Empty;

    /// <summary>
    /// The Size of the message
    /// </summary>
    public int MessageSize { get; set; }

    /// <summary>
    /// TimeStamp when cached
    /// </summary>
    public DateTime TimeStamp { get; init; } = DateTime.UtcNow;

    /// <summary>
    /// Server which delivered this response
    /// </summary>
    public IPEndPoint Server { get; set; }

    public Response()
    {
        Server = new IPEndPoint(0,0);
        Header = new Header();
    }

    public Response(IPEndPoint iPEndPoint, byte[] data)
    {
        Server = iPEndPoint;
        MessageSize = data.Length;
        var rr = new RecordReader(data);

        Header = new Header(rr);

        for (var intI = 0; intI < Header.QuestionCount; intI++)
        {
            Questions.Add(new Question(rr));
        }

        for (var intI = 0; intI < Header.AnswerCount; intI++)
        {
            Answers.Add(new AnswerResourceRecord(rr));
        }

        for (var intI = 0; intI < Header.NameServerCount; intI++)
        {
            Authorities.Add(new AuthorityResourceRecord(rr));
        }

        for (var intI = 0; intI < Header.AdditionRecordCount; intI++)
        {
            Additionals.Add(new AdditionalResourceRecord(rr));
        }
    }

    /// <summary>
    /// List of records of the given type in Response.Answers
    /// </summary>
    public IEnumerable<T> GetRecordsOfType<T>()
    where T: Record
    {
        return Answers.Select(x => x.Record).OfType<T>();
    }

    public IEnumerable<ResourceRecord> GetResourceRecord()
    {
        foreach (var rr in Answers)
        {
            yield return rr;
        }
        foreach (var rr in Authorities)
        {
            yield return rr;
        }
        foreach (var rr in Additionals)
        {
            yield return rr;
        }
    }
}