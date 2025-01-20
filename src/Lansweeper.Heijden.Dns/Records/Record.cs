// Stuff records are made of

namespace Lansweeper.Heijden.Dns.Records;

public abstract class Record
{
    /// <summary>
    /// The Resource Record this RDATA record belongs to
    /// </summary>
    public RR? RR { get; set; }
}