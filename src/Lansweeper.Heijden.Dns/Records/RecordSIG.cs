// ReSharper disable ConvertToPrimaryConstructor
// Sequence of the reads is important

namespace Lansweeper.Heijden.Dns.Records;

/*
 * http://www.ietf.org/rfc/rfc2535.txt
 * 4.1 SIG RDATA Format

   The RDATA portion of a SIG RR is as shown below.  The integrity of
   the RDATA information is protected by the signature field.

                           1 1 1 1 1 1 1 1 1 1 2 2 2 2 2 2 2 2 2 2 3 3
       0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1
      +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
      |        type covered           |  algorithm    |     labels    |
      +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
      |                         original TTL                          |
      +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
      |                      signature expiration                     |
      +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
      |                      signature inception                      |
      +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
      |            key  tag           |                               |
      +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+         signer's name         +
      |                                                               /
      +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-/
      /                                                               /
      /                            signature                          /
      /                                                               /
      +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+


*/
public class RecordSIG : Record
{
    public ushort TypeCovered { get; set; }
    public byte Algorithm { get; set; }
    public byte Labels { get; set; }
    public uint OriginalTtl { get; set; }
    public uint SignatureExpiration { get; set; }
    public uint SignatureInception { get; set; }
    public ushort KeyTag { get; set; }
    public string SignersName { get; set; }
    public string Signature { get; set; }

    public RecordSIG(RecordReader rr)
    {
        TypeCovered = rr.ReadUInt16();
        Algorithm = rr.ReadByte();
        Labels = rr.ReadByte();
        OriginalTtl = rr.ReadUInt32();
        SignatureExpiration = rr.ReadUInt32();
        SignatureInception = rr.ReadUInt32();
        KeyTag = rr.ReadUInt16();
        SignersName = rr.ReadDomainName();
        Signature = rr.ReadString();
    }

    public override string ToString()
    {
        return $"{TypeCovered} {Algorithm} {Labels} {OriginalTtl} {SignatureExpiration} {SignatureInception} {KeyTag} {SignersName} \"{Signature}\"";
    }
}