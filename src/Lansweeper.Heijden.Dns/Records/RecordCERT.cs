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
    public ushort TYPE { get; set; }
    public ushort KEYTAG { get; set; }  //Format
    public byte ALGORITHM { get; set; }
    public string PUBLICKEY { get; set; }
    public byte[] RAWKEY { get; set; }

    public RecordCERT(RecordReader rr)
    {
        // re-read length
        var RDLENGTH = rr.ReadUInt16(-2);

        TYPE = rr.ReadUInt16();
        KEYTAG = rr.ReadUInt16();
        ALGORITHM = rr.ReadByte();
        var length = RDLENGTH - 5;
        RAWKEY = rr.ReadBytes(length);
        PUBLICKEY = Convert.ToBase64String(RAWKEY);
    }

    public override string ToString()
    {
        return PUBLICKEY;
    }
}