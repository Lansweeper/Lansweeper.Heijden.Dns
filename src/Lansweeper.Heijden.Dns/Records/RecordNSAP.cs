// ReSharper disable ConvertToPrimaryConstructor
// Sequence of the reads is important

using System.Text;

namespace Lansweeper.Heijden.Dns.Records;

/*
 * http://tools.ietf.org/rfc/rfc1348.txt  
 * http://tools.ietf.org/html/rfc1706
 * 
 *	          |--------------|
              | <-- IDP -->  |
              |--------------|-------------------------------------|
              | AFI |  IDI   |            <-- DSP -->              |
              |-----|--------|-------------------------------------|
              | 47  |  0005  | DFI | AA |Rsvd | RD |Area | ID |Sel |
              |-----|--------|-----|----|-----|----|-----|----|----|
       octets |  1  |   2    |  1  | 3  |  2  | 2  |  2  | 6  | 1  |
              |-----|--------|-----|----|-----|----|-----|----|----|

                    IDP    Initial Domain Part
                    AFI    Authority and Format Identifier
                    IDI    Initial Domain Identifier
                    DSP    Domain Specific Part
                    DFI    DSP Format Identifier
                    AA     Administrative Authority
                    Rsvd   Reserved
                    RD     Routing Domain Identifier
                    Area   Area Identifier
                    ID     System Identifier
                    SEL    NSAP Selector

                  Figure 1: GOSIP Version 2 NSAP structure.


 */
public class RecordNSAP : Record
{
    public ushort Length { get; set; }
    public byte[] NsapAddress { get; set; }

    public RecordNSAP(RecordReader rr)
    {
        Length = rr.ReadUInt16();
        NsapAddress = rr.ReadBytes(Length);
    }

    public override string ToString()
    {
        var sb = new StringBuilder();
        sb.Append($"{Length} ");
        foreach (var t in NsapAddress)
        {
            sb.Append($"{t:X00}");
        }
        return sb.ToString();
    }

    public string ToGOSIPV2()
    {
        return string.Format("{0:X}.{1:X}.{2:X}.{3:X}.{4:X}.{5:X}.{6:X}{7:X}.{8:X}",
            NsapAddress[0],							// AFI
            NsapAddress[1]  << 8  | NsapAddress[2],	// IDI
            NsapAddress[3],							// DFI
            NsapAddress[4]  << 16 | NsapAddress[5] << 8 | NsapAddress[6], // AA
            NsapAddress[7]  << 8  | NsapAddress[8],	// Rsvd
            NsapAddress[9]  << 8  | NsapAddress[10],// RD
            NsapAddress[11] << 8  | NsapAddress[12],// Area
            NsapAddress[13] << 16 | NsapAddress[14] << 8 | NsapAddress[15], // ID-High
            NsapAddress[16] << 16 | NsapAddress[17] << 8 | NsapAddress[18], // ID-Low
            NsapAddress[19]);
    }
}