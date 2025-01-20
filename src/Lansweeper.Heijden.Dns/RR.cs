using Heijden.Dns.Enums;
using System;
using Type = Heijden.Dns.Enums.Type;

namespace Heijden.DNS;

#region RFC info
/*
3.2. RR definitions

3.2.1. Format

All RRs have the same top level format shown below:

                                    1  1  1  1  1  1
      0  1  2  3  4  5  6  7  8  9  0  1  2  3  4  5
    +--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+
    |                                               |
    /                                               /
    /                      NAME                     /
    |                                               |
    +--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+
    |                      TYPE                     |
    +--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+
    |                     CLASS                     |
    +--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+
    |                      TTL                      |
    |                                               |
    +--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+
    |                   RDLENGTH                    |
    +--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--|
    /                     RDATA                     /
    /                                               /
    +--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+


where:

NAME            an owner name, i.e., the name of the node to which this
                resource record pertains.

TYPE            two octets containing one of the RR TYPE codes.

CLASS           two octets containing one of the RR CLASS codes.

TTL             a 32 bit signed integer that specifies the time interval
                that the resource record may be cached before the source
                of the information should again be consulted.  Zero
                values are interpreted to mean that the RR can only be
                used for the transaction in progress, and should not be
                cached.  For example, SOA records are always distributed
                with a zero TTL to prohibit caching.  Zero values can
                also be used for extremely volatile data.

RDLENGTH        an unsigned 16 bit integer that specifies the length in
                octets of the RDATA field.

RDATA           a variable length string of octets that describes the
                resource.  The format of this information varies
                according to the TYPE and CLASS of the resource record.
*/
#endregion

/// <summary>
/// Resource Record (rfc1034 3.6.)
/// </summary>
public class RR
{
    /// <summary>
    /// The name of the node to which this resource record pertains
    /// </summary>
    public string NAME { get; set; }

    /// <summary>
    /// Specifies type of resource record
    /// </summary>
    public Type Type { get; set; }

    /// <summary>
    /// Specifies type class of resource record, mostly IN but can be CS, CH or HS 
    /// </summary>
    public Class Class { get; set; }

    /// <summary>
    /// Time to live, the time interval that the resource record may be cached
    /// </summary>
    public uint TTL
    {
        get
        {
            return (uint)Math.Max(0, _ttl - TimeLived);
        }
        set
        {
            _ttl = value;
        }
    }
    private uint _ttl;

    /// <summary>
    /// Specifies the length in octets of the RDATA field.
    /// </summary>
    public ushort RDLENGTH { get; set; }

    /// <summary>
    /// One of the Record* classes
    /// </summary>
    public Record RECORD { get; set; }

    public int TimeLived { get; set; }

    public RR(RecordReader rr)
    {
        TimeLived = 0;
        NAME = rr.ReadDomainName();
        Type = (Type)rr.ReadUInt16();
        Class = (Class)rr.ReadUInt16();
        TTL = rr.ReadUInt32();
        RDLENGTH = rr.ReadUInt16();
        RECORD = rr.ReadRecord(Type, RDLENGTH);
        RECORD.RR = this;
    }

    public override string ToString()
    {
        return $"{NAME,-32} {TTL}\t{Class}\t{Type}\t{RECORD}";
    }
}

public class AnswerRR(RecordReader br) : RR(br);

public class AuthorityRR(RecordReader br) : RR(br);

public class AdditionalRR(RecordReader br) : RR(br);