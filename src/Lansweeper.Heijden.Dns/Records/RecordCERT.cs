namespace Lansweeper.Heijden.Dns.Records;

/*

 CERT RR
 *                     1 1 1 1 1 1 1 1 1 1 2 2 2 2 2 2 2 2 2 2 3 3
   0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1
   +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
   |             type              |             key tag           |
   +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
   |   algorithm   |                                               /
   +---------------+            certificate or CRL                 /
   /                                                               /
   +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-|
 */

public class RecordCERT : Record
{
    public ushort Type { get; set; }
    public ushort KeyTag { get; set; }  //Format
    public byte Algorithm { get; set; }
    public string PublicKey { get; set; }

    public RecordCERT(RecordReader rr)
    {
        // re-read length
        var RDLENGTH = rr.ReadUInt16(-2);

        Type = rr.ReadUInt16();
        KeyTag = rr.ReadUInt16();
        Algorithm = rr.ReadByte();
        var length = RDLENGTH - 5;
        PublicKey = Convert.ToBase64String(rr.ReadBytes(length));
    }

    public override string ToString()
    {
        return PublicKey;
    }
}