// ReSharper disable ConvertToPrimaryConstructor
// Sequence of the reads is important

using System.Text;

namespace Lansweeper.Heijden.Dns.Records;

/*
 * http://www.ietf.org/rfc/rfc1876.txt
 * 
2. RDATA Format

       MSB                                           LSB
       +--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+
      0|        VERSION        |         SIZE          |
       +--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+
      2|       HORIZ PRE       |       VERT PRE        |
       +--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+
      4|                   LATITUDE                    |
       +--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+
      6|                   LATITUDE                    |
       +--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+
      8|                   LONGITUDE                   |
       +--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+
     10|                   LONGITUDE                   |
       +--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+
     12|                   ALTITUDE                    |
       +--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+
     14|                   ALTITUDE                    |
       +--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+--+

where:

VERSION      Version number of the representation.  This must be zero.
             Implementations are required to check this field and make
             no assumptions about the format of unrecognized versions.

SIZE         The diameter of a sphere enclosing the described entity, in
             centimeters, expressed as a pair of four-bit unsigned
             integers, each ranging from zero to nine, with the most
             significant four bits representing the base and the second
             number representing the power of ten by which to multiply
             the base.  This allows sizes from 0e0 (<1cm) to 9e9
             (90,000km) to be expressed.  This representation was chosen
             such that the hexadecimal representation can be read by
             eye; 0x15 = 1e5.  Four-bit values greater than 9 are
             undefined, as are values with a base of zero and a non-zero
             exponent.

             Since 20000000m (represented by the value 0x29) is greater
             than the equatorial diameter of the WGS 84 ellipsoid
             (12756274m), it is therefore suitable for use as a
             "worldwide" size.

HORIZ PRE    The horizontal precision of the data, in centimeters,
             expressed using the same representation as SIZE.  This is
             the diameter of the horizontal "circle of error", rather
             than a "plus or minus" value.  (This was chosen to match
             the interpretation of SIZE; to get a "plus or minus" value,
             divide by 2.)

VERT PRE     The vertical precision of the data, in centimeters,
             expressed using the sane representation as for SIZE.  This
             is the total potential vertical error, rather than a "plus
             or minus" value.  (This was chosen to match the
             interpretation of SIZE; to get a "plus or minus" value,
             divide by 2.)  Note that if altitude above or below sea
             level is used as an approximation for altitude relative to
             the [WGS 84] ellipsoid, the precision value should be
             adjusted.

LATITUDE     The latitude of the center of the sphere described by the
             SIZE field, expressed as a 32-bit integer, most significant
             octet first (network standard byte order), in thousandths
             of a second of arc.  2^31 represents the equator; numbers
             above that are north latitude.

LONGITUDE    The longitude of the center of the sphere described by the
             SIZE field, expressed as a 32-bit integer, most significant
             octet first (network standard byte order), in thousandths
             of a second of arc, rounded away from the prime meridian.
             2^31 represents the prime meridian; numbers above that are
             east longitude.

ALTITUDE     The altitude of the center of the sphere described by the
             SIZE field, expressed as a 32-bit integer, most significant
             octet first (network standard byte order), in centimeters,
             from a base of 100,000m below the [WGS 84] reference
             spheroid used by GPS (semimajor axis a=6378137.0,
             reciprocal flattening rf=298.257223563).  Altitude above
             (or below) sea level may be used as an approximation of
             altitude relative to the the [WGS 84] spheroid, though due
             to the Earth's surface not being a perfect spheroid, there
             will be differences.  (For example, the geoid (which sea
             level approximates) for the continental US ranges from 10
             meters to 50 meters below the [WGS 84] spheroid.
             Adjustments to ALTITUDE and/or VERT PRE will be necessary
             in most cases.  The Defense Mapping Agency publishes geoid
             height values relative to the [WGS 84] ellipsoid.

 */
public class RecordLOC : Record
{
    private const uint Mid = 2147483648; // 2^31

    public byte Version { get; set; }
    public byte Size { get; set; }
    public byte HorizontalPrecision { get; set; }
    public byte VerticalPrecision { get; set; }
    public uint Latitude { get; set; }
    public uint Longitude { get; set; }
    public uint Altitude { get; set; }

    public RecordLOC(RecordReader rr)
    {
        Version = rr.ReadByte(); // must be 0!
        Size = rr.ReadByte();
        HorizontalPrecision = rr.ReadByte();
        VerticalPrecision = rr.ReadByte();
        Latitude = rr.ReadUInt32();
        Longitude = rr.ReadUInt32();
        Altitude = rr.ReadUInt32();
    }

    private static string SizeToString(byte s)
    {
        var strUnit = "cm";
        var intBase = s >> 4;
        var intPow = s & 0x0f;
        if (intPow >= 2)
        {
            intPow -= 2;
            strUnit = "m";
        }
        /*
        if (intPow >= 3)
        {
            intPow -= 3;
            strUnit = "km";
        }
        */
        var sb = new StringBuilder();
        sb.AppendFormat("{0}", intBase);
        for (; intPow > 0; intPow--)
        {
            sb.Append('0');
        }
        sb.Append(strUnit);
        return sb.ToString();
    }

    private static string LonToTime(uint r)
    {
        var dir = 'E';
        if (r > Mid)
        {
            dir = 'W';
            r -= Mid;
        }
        var h = r / (360000.0 * 10.0);
        var m = 60.0 * (h - (int)h);
        var s = 60.0 * (m - (int)m);
        return $"{(int)h} {(int)m} {s:0.000} {dir}";
    }

    private static string ToTime(uint r, char below,char above)
    {
        var dir = '?';
        if (r > Mid)
        {
            dir = above;
            r -= Mid;
        }
        else
        {
            dir = below;
            r = Mid - r;
        }
        var h = r / (360000.0 * 10.0);
        var m = 60.0 * (h - (int)h);
        var s = 60.0 * (m - (int)m);
        return $"{(int)h} {(int)m} {s:0.000} {dir}";
    }

    private static string ToAlt(uint a)
    {
        var alt = (a / 100.0) - 100000.00;
        return $"{alt:0.00}m";
    }

    public override string ToString()
    {
        return $"{ToTime(Latitude, 'S', 'N')} {ToTime(Longitude, 'W', 'E')} " +
               $"{ToAlt(Altitude)} {SizeToString(Size)} {SizeToString(HorizontalPrecision)} {SizeToString(VerticalPrecision)}";
    }
}