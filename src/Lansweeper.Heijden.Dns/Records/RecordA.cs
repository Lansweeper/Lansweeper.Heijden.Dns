using System.Net;

namespace Lansweeper.Heijden.Dns.Records;

/*
 3.4.1. A RDATA format

    +--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+
    |                    ADDRESS                    |
    +--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+

where:

ADDRESS         A 32 bit Internet address.

Hosts that have multiple Internet addresses will have multiple A
records.
 * 
 */

public class RecordA(RecordReader rr) : Record
{
    public IPAddress Address { get; set; } = new(rr.ReadSpan(4));

    public override string ToString()
    {
        return Address.ToString();
    }
}