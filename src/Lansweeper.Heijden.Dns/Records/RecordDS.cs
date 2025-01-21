namespace Lansweeper.Heijden.Dns.Records;

/*
 * http://tools.ietf.org/rfc/rfc3658.txt
 *
2.4.  Wire Format of the DS record

   The DS (type=43) record contains these fields: key tag, algorithm,
   digest type, and the digest of a public key KEY record that is
   allowed and/or used to sign the child's apex KEY RRset.  Other keys
   MAY sign the child's apex KEY RRset.

                        1 1 1 1 1 1 1 1 1 1 2 2 2 2 2 2 2 2 2 2 3 3
    0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1
   +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
   |           key tag             |  algorithm    |  Digest type  |
   +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
   |                digest  (length depends on type)               |
   +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
   |                (SHA-1 digest is 20 bytes)                     |
   +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
   |                                                               |
   +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-|
   |                                                               |
   +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-|
   |                                                               |
   +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+

 */

public class RecordDS : Record
{
    public ushort KeyTag { get; set; }
    public byte Algorithm { get; set; }
    public byte DigestType { get; set; }
    public string Digest { get; set; }

    public RecordDS(RecordReader rr)
    {
        var length = rr.ReadUInt16(-2);
        KeyTag = rr.ReadUInt16();
        Algorithm = rr.ReadByte();
        DigestType = rr.ReadByte();
        length -= 4;
        Digest = Convert.ToBase64String(rr.ReadBytes(length));
    }

    public override string ToString()
    {
        return $"{KeyTag} {Algorithm} {DigestType} {Digest}";
    }
}