// ReSharper disable ConvertToPrimaryConstructor
// Sequence of the reads is important

namespace Lansweeper.Heijden.Dns.Records;

/*
 * http://www.faqs.org/rfcs/rfc2915.html
 * 
 8. DNS Packet Format

         The packet format for the NAPTR record is:

                                          1  1  1  1  1  1
            0  1  2  3  4  5  6  7  8  9  0  1  2  3  4  5
          +--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+
          |                     ORDER                     |
          +--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+
          |                   PREFERENCE                  |
          +--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+
          /                     FLAGS                     /
          +--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+
          /                   SERVICES                    /
          +--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+
          /                    REGEXP                     /
          +--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+
          /                  REPLACEMENT                  /
          /                                               /
          +--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+

    where:

   FLAGS A <character-string> which contains various flags.

   SERVICES A <character-string> which contains protocol and service
      identifiers.

   REGEXP A <character-string> which contains a regular expression.

   REPLACEMENT A <domain-name> which specifies the new value in the
      case where the regular expression is a simple replacement
      operation.

   <character-string> and <domain-name> as used here are defined in
   RFC1035 [1].

 */
public class RecordNAPTR : Record
{
    public ushort Order { get; set; }
    public ushort Preference { get; set; }
    public string Flags { get; set; }
    public string Services { get; set; }
    public string RegExp { get; set; }
    public string Replacement { get; set; }

    public RecordNAPTR(RecordReader rr)
    {
        Order = rr.ReadUInt16();
        Preference = rr.ReadUInt16();
        Flags = rr.ReadString();
        Services = rr.ReadString();
        RegExp = rr.ReadString();
        Replacement = rr.ReadDomainName();
    }

    public override string ToString()
    {
        return $"{Order} {Preference} \"{Flags}\" \"{Services}\" \"{RegExp}\" {Replacement}";
    }
}