// ReSharper disable ConvertToPrimaryConstructor
// Sequence of the reads is important

namespace Lansweeper.Heijden.Dns.Records;

/*
 * http://tools.ietf.org/rfc/rfc2230.txt
 * 
 * 3.1 KX RDATA format

   The KX DNS record has the following RDATA format:

    +--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+
    |                  PREFERENCE                   |
    +--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+
    /                   EXCHANGER                   /
    /                                               /
    +--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+

   where:

   PREFERENCE      A 16 bit non-negative integer which specifies the
                   preference given to this RR among other KX records
                   at the same owner.  Lower values are preferred.

   EXCHANGER       A <domain-name> which specifies a host willing to
                   act as a mail exchange for the owner name.

   KX records MUST cause type A additional section processing for the
   host specified by EXCHANGER.  In the event that the host processing
   the DNS transaction supports IPv6, KX records MUST also cause type
   AAAA additional section processing.

   The KX RDATA field MUST NOT be compressed.

 */
public class RecordKX : Record, IComparable
{
    public ushort Preference { get; set; }
    public string Exchanger { get; set; }

    public RecordKX(RecordReader rr)
    {
        Preference = rr.ReadUInt16();
        Exchanger = rr.ReadDomainName();
    }

    public override string ToString()
    {
        return $"{Preference} {Exchanger}";
    }

    public int CompareTo(object? objA)
    {
        if (objA is not RecordKX recordKX) return -1;
        
        if (Preference > recordKX.Preference) return 1;
        if (Preference < recordKX.Preference) return -1;

        // they are the same, now compare case-insensitive names
        return string.Compare(Exchanger, recordKX.Exchanger, StringComparison.InvariantCultureIgnoreCase);
    }
}