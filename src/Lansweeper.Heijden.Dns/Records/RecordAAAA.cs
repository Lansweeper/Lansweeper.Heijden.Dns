using System.Net;

namespace Lansweeper.Heijden.Dns.Records;

#region Rfc info
/*
2.2 AAAA data format

   A 128 bit IPv6 address is encoded in the data portion of an AAAA
   resource record in network byte order (high-order byte first).
 */
#endregion

public class RecordAAAA(RecordReader rr) : Record
{
    public IPAddress Address { get; set; } = new(rr.ReadSpan(16));

    public override string ToString()
    {
        return Address.ToString();
    }
}