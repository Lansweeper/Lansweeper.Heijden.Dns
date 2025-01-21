// ReSharper disable ConvertToPrimaryConstructor
// Sequence of the reads is important

namespace Lansweeper.Heijden.Dns.Records;

/*
 * http://tools.ietf.org/rfc/rfc2930.txt
 * 
2. The TKEY Resource Record

   The TKEY resource record (RR) has the structure given below.  Its RR
   type code is 249.

      Field       Type         Comment
      -----       ----         -------
       Algorithm:   domain
       Inception:   u_int32_t
       Expiration:  u_int32_t
       Mode:        u_int16_t
       Error:       u_int16_t
       Key Size:    u_int16_t
       Key Data:    octet-stream
       Other Size:  u_int16_t
       Other Data:  octet-stream  undefined by this specification

 */
public class RecordTKEY : Record
{
    public string Algorithm { get; set; }
    public uint Inception { get; set; }
    public uint Expiration { get; set; }
    public ushort Mode { get; set; }
    public ushort Error { get; set; }
    public ushort KeySize { get; set; }
    public byte[] KeyData { get; set; }
    public ushort OtherSize { get; set; }
    public byte[] OtherData { get; set; }

    public RecordTKEY(RecordReader rr)
    {
        Algorithm = rr.ReadDomainName();
        Inception = rr.ReadUInt32();
        Expiration = rr.ReadUInt32();
        Mode = rr.ReadUInt16();
        Error = rr.ReadUInt16();
        KeySize = rr.ReadUInt16();
        KeyData = rr.ReadBytes(KeySize);
        OtherSize = rr.ReadUInt16();
        OtherData = rr.ReadBytes(OtherSize);
    }

    public override string ToString()
    {
        return $"{Algorithm} {Inception} {Expiration} {Mode} {Error}";
    }
}