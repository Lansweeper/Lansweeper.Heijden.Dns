namespace Lansweeper.Heijden.Dns.Enums;

/*
 * 3.2.5. QCLASS values
 *
 * QCLASS fields appear in the question section of a query.  QCLASS values
 * are a superset of CLASS values; every CLASS is a valid QCLASS.  In
 * addition to CLASS values, the following QCLASSes are defined:
 *
 *		QCLASS		value			meaning
 */
public enum QClass : ushort
{
    IN = Class.IN,		// the Internet
    CS = Class.CS,		// the CSNET class (Obsolete - used only for examples in some obsolete RFCs)
    CH = Class.CH,		// the CHAOS class
    HS = Class.HS,		// Hesiod [Dyer 87]

    ANY = 255			// any class
}