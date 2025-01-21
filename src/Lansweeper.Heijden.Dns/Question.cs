using Lansweeper.Heijden.Dns.Enums;

namespace Lansweeper.Heijden.Dns;

#region Rfc 1034/1035
/*
4.1.2. Question section format

The question section is used to carry the "question" in most queries,
i.e., the parameters that define what is being asked.  The section
contains QDCOUNT (usually 1) entries, each of the following format:

                                    1  1  1  1  1  1
      0  1  2  3  4  5  6  7  8  9  0  1  2  3  4  5
    +--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+
    |                                               |
    /                     QNAME                     /
    /                                               /
    +--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+
    |                     QTYPE                     |
    +--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+
    |                     QCLASS                    |
    +--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+

where:

QNAME           a domain name represented as a sequence of labels, where
                each label consists of a length octet followed by that
                number of octets.  The domain name terminates with the
                zero length octet for the null label of the root.  Note
                that this field may be an odd number of octets; no
                padding is used.

QTYPE           a two octet code which specifies the type of the query.
                The values for this field include all codes valid for a
                TYPE field, together with some more general codes which
                can match more than one type of RR.


QCLASS          a two octet code that specifies the class of the query.
                For example, the QCLASS field is IN for the Internet.
*/
#endregion

public class Question
{
    private string _name = string.Empty;

    public string Name
    {
        get => _name;
        set
        {
            _name = value;
            if (!_name.EndsWith('.'))
            {
                _name += ".";
            }
        }
    }
    public QType Type { get; set; }
    public QClass Class { get; set; }

    public Question(string name, QType type, QClass @class)
    {
        Name = name;
        Type = type;
        Class = @class;
    }

    public Question(RecordReader rr)
    {
        Name = rr.ReadDomainName();
        Type = (QType)rr.ReadUInt16();
        Class = (QClass)rr.ReadUInt16();
    }

    public IEnumerable<byte> GetData()
    {
        return RecordWriter.WriteName(Name).Concat(RecordWriter.WriteShort((ushort)Type)).Concat(RecordWriter.WriteShort((ushort)Class));
    }

    public override string ToString()
    {
        return $"{Name,-32}\t{Class}\t{Type}";
    }
}