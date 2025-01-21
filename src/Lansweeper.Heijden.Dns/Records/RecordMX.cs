// ReSharper disable ConvertToPrimaryConstructor
// Sequence of the reads is important

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
    public ushort Preference { get; set; }
    public string Exchange { get; set; }

    public RecordMX(RecordReader rr)
    {
        Preference = rr.ReadUInt16();
        Exchange = rr.ReadDomainName();
    }

    public override string ToString()
    {
        return $"{Preference} {Exchange}";
    }

    public int CompareTo(object? objA)
    {
        if (objA is not RecordMX recordMX) return -1;

        if (Preference > recordMX.Preference) return 1;
        if (Preference < recordMX.Preference) return -1;

        // they are the same, now compare case insensitive names
        return string.Compare(Exchange, recordMX.Exchange, StringComparison.InvariantCultureIgnoreCase);
    }
}