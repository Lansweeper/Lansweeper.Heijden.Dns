namespace Lansweeper.Heijden.Dns.Records;

/*
3.3.3. MB RDATA format (EXPERIMENTAL)

    +--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+
    /                   MADNAME                     /
    /                                               /
    +--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+

where:

MADNAME         A <domain-name> which specifies a host which has the
                specified mailbox.

MB records cause additional section processing which looks up an A type
RRs corresponding to MADNAME.
*/
public class RecordMB(RecordReader rr) : Record
{
    public string MadName { get; set; } = rr.ReadDomainName();

    public override string ToString()
    {
        return MadName;
    }
}