namespace Lansweeper.Heijden.Dns.Records;

/*
 * 
3.3.1. CNAME RDATA format

    +--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+
    /                     CNAME                     /
    /                                               /
    +--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+

where:

CNAME           A <domain-name> which specifies the canonical or primary
                name for the owner.  The owner name is an alias.

CNAME RRs cause no additional section processing, but name servers may
choose to restart the query at the canonical name in certain cases.  See
the description of name server logic in [RFC-1034] for details.

 * 
 */

public class RecordCNAME : Record
{
    public string CNAME { get; set; }

    public RecordCNAME(RecordReader rr)
    {
        CNAME = rr.ReadDomainName();
    }

    public override string ToString()
    {
        return CNAME;
    }
}