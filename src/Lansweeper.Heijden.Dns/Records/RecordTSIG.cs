// ReSharper disable ConvertToPrimaryConstructor
// Sequence of the reads is important

namespace Lansweeper.Heijden.Dns.Records;

/*
 * http://www.ietf.org/rfc/rfc2845.txt
 * 
 * Field Name       Data Type      Notes
      --------------------------------------------------------------
      Algorithm Name   domain-name    Name of the algorithm
                                      in domain name syntax.
      Time Signed      u_int48_t      seconds since 1-Jan-70 UTC.
      Fudge            u_int16_t      seconds of error permitted
                                      in Time Signed.
      MAC Size         u_int16_t      number of octets in MAC.
      MAC              octet stream   defined by Algorithm Name.
      Original ID      u_int16_t      original message ID
      Error            u_int16_t      expanded RCODE covering
                                      TSIG processing.
      Other Len        u_int16_t      length, in octets, of
                                      Other Data.
      Other Data       octet stream   empty unless Error == BADTIME

 */
public class RecordTSIG : Record
{
    public string AlgorithmName { get; set; }
    public long TimeSigned { get; set; }
    public ushort Fudge { get; set; }
    public ushort MacSize { get; set; }
    public byte[] Mac { get; set; }
    public ushort OriginalId { get; set; }
    public ushort Error { get; set; }
    public ushort OtherLen { get; set; }
    public byte[] OtherData { get; set; }

    public RecordTSIG(RecordReader rr)
    {
        AlgorithmName = rr.ReadDomainName();
        TimeSigned = rr.ReadUInt32() << 32 | rr.ReadUInt32();
        Fudge = rr.ReadUInt16();
        MacSize = rr.ReadUInt16();
        Mac = rr.ReadBytes(MacSize);
        OriginalId = rr.ReadUInt16();
        Error = rr.ReadUInt16();
        OtherLen = rr.ReadUInt16();
        OtherData = rr.ReadBytes(OtherLen);
    }

    public override string ToString()
    {
        var dateTime = DateTime.UnixEpoch.AddSeconds(TimeSigned);
        return $"{AlgorithmName} {dateTime.ToShortDateString()} {dateTime.ToShortTimeString()} {Fudge} {OriginalId} {Error}";
    }
}