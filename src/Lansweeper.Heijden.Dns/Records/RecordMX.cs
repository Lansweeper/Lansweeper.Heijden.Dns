namespace Lansweeper.Heijden.Dns.Records;

/*
3.3.9. MX RDATA format

    +--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+
    |                  PREFERENCE                   |
    +--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+
    /                   EXCHANGE                    /
    /                                               /
    +--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+

where:

PREFERENCE      A 16 bit integer which specifies the preference given to
                this RR among others at the same owner.  Lower values
                are preferred.

EXCHANGE        A <domain-name> which specifies a host willing to act as
                a mail exchange for the owner name.

MX records cause type A additional section processing for the host
specified by EXCHANGE.  The use of MX RRs is explained in detail in
[RFC-974].
*/
public class RecordMX : Record, IComparable
{
    public ushort PREFERENCE { get; set; }
    public string EXCHANGE { get; set; }

    public RecordMX(RecordReader rr)
    {
        PREFERENCE = rr.ReadUInt16();
        EXCHANGE = rr.ReadDomainName();
    }

    public override string ToString()
    {
        return $"{PREFERENCE} {EXCHANGE}";
    }

    public int CompareTo(object? objA)
    {
        if (objA is not RecordMX recordMX) return -1;

        if (PREFERENCE > recordMX.PREFERENCE) return 1;
        if (PREFERENCE < recordMX.PREFERENCE) return -1;

        // they are the same, now compare case insensitive names
        return string.Compare(EXCHANGE, recordMX.EXCHANGE, StringComparison.InvariantCultureIgnoreCase);
    }
}