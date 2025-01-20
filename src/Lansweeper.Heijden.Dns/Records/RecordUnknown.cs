using Type = Lansweeper.Heijden.Dns.Enums.Type;

namespace Lansweeper.Heijden.Dns.Records;

public class RecordUnknown : Record
{
    public Type Type { get; }
    public byte[] RDATA { get; set; }

    public RecordUnknown(RecordReader rr, Type type)
    {
        Type = type;
        // re-read length
        var length = rr.ReadUInt16(-2);
        RDATA = rr.ReadBytes(length);
    }

    public override string ToString()
    {
        return $"{Type}: not-used";
    }
}