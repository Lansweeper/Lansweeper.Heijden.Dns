

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
namespace Lansweeper.Heijden.Dns.Records;

public class RecordTSIG : Record
{
    public string ALGORITHMNAME { get; set; }
    public long TIMESIGNED { get; set; }
    public ushort FUDGE { get; set; }
    public ushort MACSIZE { get; set; }
    public byte[] MAC { get; set; }
    public ushort ORIGINALID { get; set; }
    public ushort ERROR { get; set; }
    public ushort OTHERLEN { get; set; }
    public byte[] OTHERDATA { get; set; }

    public RecordTSIG(RecordReader rr)
    {
        ALGORITHMNAME = rr.ReadDomainName();
        TIMESIGNED = rr.ReadUInt32() << 32 | rr.ReadUInt32();
        FUDGE = rr.ReadUInt16();
        MACSIZE = rr.ReadUInt16();
        MAC = rr.ReadBytes(MACSIZE);
        ORIGINALID = rr.ReadUInt16();
        ERROR = rr.ReadUInt16();
        OTHERLEN = rr.ReadUInt16();
        OTHERDATA = rr.ReadBytes(OTHERLEN);
    }

    public override string ToString()
    {
        var dateTime = DateTime.UnixEpoch.AddSeconds(TIMESIGNED);
        return $"{ALGORITHMNAME} {dateTime.ToShortDateString()} {dateTime.ToShortTimeString()} {FUDGE} {ORIGINALID} {ERROR}";
    }
}