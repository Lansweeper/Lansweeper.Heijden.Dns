namespace Lansweeper.Heijden.Dns.Enums;

/*
 * 3.2.4. CLASS values
 *
 * CLASS fields appear in resource records.  The following CLASS mnemonics
 *and values are defined:
 *
 *		CLASS		value			meaning
 */
public enum Class : ushort
{
    IN = 1,				// the Internet
    CS = 2,				// the CSNET class (Obsolete - used only for examples in some obsolete RFCs)
    CH = 3,				// the CHAOS class
    HS = 4				// Hesiod [Dyer 87]
}