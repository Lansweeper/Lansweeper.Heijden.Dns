// ReSharper disable ConvertToPrimaryConstructor
// Sequence of the reads is important

namespace Lansweeper.Heijden.Dns.Records;

/*
3.3.10. NULL RDATA format (EXPERIMENTAL)

    +--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+
    /                  <anything>                   /
    /                                               /
    +--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+

Anything at all may be in the RDATA field so long as it is 65535 octets
or less.

NULL records cause no additional section processing.  NULL RRs are not
allowed in master files.  NULLs are used as placeholders in some
experimental extensions of the DNS.
*/
public class RecordNULL : Record
{
    public byte[] Anything { get; set; }

    public RecordNULL(RecordReader rr)
    {
        rr.Position -= 2;
        // re-read length
        var RDLENGTH = rr.ReadUInt16();
        Anything = new byte[RDLENGTH];
        Anything = rr.ReadBytes(RDLENGTH);
    }

    public override string ToString()
    {
        return $"...binary data... ({Anything.Length}) bytes";
    }
}