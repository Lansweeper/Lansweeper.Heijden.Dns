using System.Text;

namespace Lansweeper.Heijden.Dns.Records;

/*
3.3.14. TXT RDATA format

    +--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+
    /                   TXT-DATA                    /
    +--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+

where:

TXT-DATA        One or more <character-string>s.

TXT RRs are used to hold descriptive text.  The semantics of the text
depends on the domain where it is found.
*/
public class RecordTXT : Record
{
    public List<string> Text { get; } = [];

    public RecordTXT(RecordReader rr, int length)
    {
        var pos = rr.Position;
        while ((rr.Position - pos) < length)
        {
            Text.Add(rr.ReadString());
        }
    }

    public override string ToString()
    {
        var stringBuilder = new StringBuilder();
        foreach (var str in this.Text)
        {
            stringBuilder.Append($"\"{str}\" ");
        }
        return stringBuilder.ToString().TrimEnd();
    }
}