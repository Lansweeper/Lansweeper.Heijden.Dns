using System.Buffers.Binary;
using System.Text;
using Lansweeper.Heijden.Dns.Records;
using Lansweeper.Heijden.Dns.Records.Obsolete;
using Type = Lansweeper.Heijden.Dns.Enums.Type;

namespace Lansweeper.Heijden.Dns;

public class RecordReader(byte[] data, int position = 0)
{
    public int Position { get; set; } = position;
        
    public byte ReadByte()
    {
        return Position >= data.Length ? (byte)0 : data[Position++];
    }

    public char ReadChar()
    {
        return (char)ReadByte();
    }

    public ushort ReadUInt16()
    {
        return BinaryPrimitives.ReadUInt16BigEndian(ReadSpan(2));
    }

    public ushort ReadUInt16(int offset)
    {
        Position += offset;
        return ReadUInt16();
    }

    public uint ReadUInt32()
    {
        return BinaryPrimitives.ReadUInt32BigEndian(ReadSpan(4));
    }

    public string ReadDomainName()
    {
        var name = new StringBuilder();
        var length = 0;

        // get  the length of the first label
        while ((length = ReadByte()) != 0)
        {
            // top 2 bits set denotes domain name compression and to reference elsewhere
            if ((length & 0xc0) == 0xc0)
            {
                // work out the existing domain name, copy this pointer
                var newRecordReader = new RecordReader(data, (length & 0x3f) << 8 | ReadByte());
                name.Append(newRecordReader.ReadDomainName());
                return name.ToString();
            }

            // if not using compression, copy a char at a time to the domain name
            while (length > 0)
            {
                name.Append(ReadChar());
                length--;
            }
            name.Append('.');
        }

        return name.Length == 0 ? "." : name.ToString();
    }

    public string ReadString()
    {
        var length = ReadByte();
        return Encoding.ASCII.GetString(ReadSpan(length));
    }

    public byte[] ReadBytes(int length)
    {
        var arr = new byte[length];
        for (var i = 0; i < length; i++)
        {
            arr[i] = ReadByte();
        }
        return arr;
    }
        
    public ReadOnlySpan<byte> ReadSpan(int length)
    {
        if (Position + length > data.Length) return ReadBytes(length).AsSpan();

        var result = data.AsSpan(Position, length);
        Position += length;
        return result;
    }

    public Record ReadRecord(Type type, int length)
    {
        return type switch
        {
            Type.A => new RecordA(this),
            Type.NS => new RecordNS(this),
            Type.MD => new RecordMD(this),
            Type.MF => new RecordMF(this),
            Type.CNAME => new RecordCNAME(this),
            Type.SOA => new RecordSOA(this),
            Type.MB => new RecordMB(this),
            Type.MG => new RecordMG(this),
            Type.MR => new RecordMR(this),
            Type.NULL => new RecordNULL(this),
            Type.WKS => new RecordWKS(this),
            Type.PTR => new RecordPTR(this),
            Type.HINFO => new RecordHINFO(this),
            Type.MINFO => new RecordMINFO(this),
            Type.MX => new RecordMX(this),
            Type.TXT => new RecordTXT(this, length),
            Type.RP => new RecordRP(this),
            Type.AFSDB => new RecordAFSDB(this),
            Type.X25 => new RecordX25(this),
            Type.ISDN => new RecordISDN(this),
            Type.RT => new RecordRT(this),
            Type.NSAP => new RecordNSAP(this),
            Type.NSAPPTR => new RecordNSAPPTR(this),
            Type.SIG => new RecordSIG(this),
            Type.KEY => new RecordKEY(this),
            Type.PX => new RecordPX(this),
            Type.GPOS => new RecordGPOS(this),
            Type.AAAA => new RecordAAAA(this),
            Type.LOC => new RecordLOC(this),
            Type.NXT => new RecordNXT(this),
            Type.EID => new RecordUnknown(this, type),
            Type.NIMLOC => new RecordUnknown(this, type),
            Type.SRV => new RecordSRV(this),
            Type.ATMA => new RecordUnknown(this, type),
            Type.NAPTR => new RecordNAPTR(this),
            Type.KX => new RecordKX(this),
            Type.CERT => new RecordCERT(this),
            Type.A6 => new RecordUnknown(this, type),
            Type.DNAME => new RecordDNAME(this),
            Type.SINK => new RecordUnknown(this, type),
            Type.OPT => new RecordUnknown(this, type),
            Type.APL => new RecordUnknown(this, type),
            Type.DS => new RecordDS(this),
            Type.SSHFP => new RecordUnknown(this, type),
            Type.IPSECKEY => new RecordUnknown(this, type),
            Type.RRSIG => new RecordUnknown(this, type),
            Type.NSEC => new RecordUnknown(this, type),
            Type.DNSKEY => new RecordUnknown(this, type),
            Type.DHCID => new RecordUnknown(this, type),
            Type.NSEC3 => new RecordUnknown(this, type),
            Type.NSEC3PARAM => new RecordUnknown(this, type),
            Type.HIP => new RecordUnknown(this, type),
            Type.SPF => new RecordUnknown(this, type),
            Type.UINFO => new RecordUnknown(this, type),
            Type.UID => new RecordUnknown(this, type),
            Type.GID => new RecordUnknown(this, type),
            Type.UNSPEC => new RecordUNSPEC(this),
            Type.TKEY => new RecordTKEY(this),
            Type.TSIG => new RecordTSIG(this),
            _ => new RecordUnknown(this, Type.Unknown)
        };
    }
}