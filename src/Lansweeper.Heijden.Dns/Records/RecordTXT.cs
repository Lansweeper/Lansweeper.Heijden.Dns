using System.Text;

#region Rfc info
/*
3.3.14. TXT RDATA format

    +--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+
    /                   TXT-DATA                    /
    +--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+

where:

TXT-DATA        One or more <character-string>s.

TXT RRs are used to hold descriptive text.  The semantics of the text
depends on the domain where it is found.
 * 
*/
#endregion
namespace Lansweeper.Heijden.Dns.Records;

public class RecordTXT : Record
{
    public List<string> TXT { get; } = [];

    public RecordTXT(RecordReader rr, int length)
    {
        var pos = rr.Position;
        while ((rr.Position - pos) < length)
        {
            TXT.Add(rr.ReadString());
        }
    }

    public override string ToString()
    {
        var stringBuilder = new StringBuilder();
        foreach (var str in this.TXT)
        {
            stringBuilder.Append($"\"{str}\" ");
        }
        return stringBuilder.ToString().TrimEnd();
    }
}