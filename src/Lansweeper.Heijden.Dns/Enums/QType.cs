namespace Heijden.Dns.Enums;

/*
 * 3.2.3. QTYPE values
 *
 * QTYPE fields appear in the question part of a query.  QTYPES are a
 * superset of TYPEs, hence all TYPEs are valid QTYPEs.  In addition, the
 * following QTYPEs are defined:
 *
 *		QTYPE		value			meaning
 */
public enum QType : ushort
{
    A = Type.A,			// a IPV4 host address
    NS = Type.NS,		// an authoritative name server
    MD = Type.MD,		// a mail destination (Obsolete - use MX)
    MF = Type.MF,		// a mail forwarder (Obsolete - use MX)
    CNAME = Type.CNAME,	// the canonical name for an alias
    SOA = Type.SOA,		// marks the start of a zone of authority
    MB = Type.MB,		// a mailbox domain name (EXPERIMENTAL)
    MG = Type.MG,		// a mail group member (EXPERIMENTAL)
    MR = Type.MR,		// a mail rename domain name (EXPERIMENTAL)
    NULL = Type.NULL,	// a null RR (EXPERIMENTAL)
    WKS = Type.WKS,		// a well known service description
    PTR = Type.PTR,		// a domain name pointer
    HINFO = Type.HINFO,	// host information
    MINFO = Type.MINFO,	// mailbox or mail list information
    MX = Type.MX,		// mail exchange
    TXT = Type.TXT,		// text strings

    RP = Type.RP,		// The Responsible Person rfc1183
    AFSDB = Type.AFSDB,	// AFS Data Base location
    X25 = Type.X25,		// X.25 address rfc1183
    ISDN = Type.ISDN,	// ISDN address rfc1183
    RT = Type.RT,		// The Route Through rfc1183

    NSAP = Type.NSAP,	// Network service access point address rfc1706
    NSAP_PTR = Type.NSAPPTR, // Obsolete, rfc1348

    SIG = Type.SIG,		// Cryptographic public key signature rfc2931 / rfc2535
    KEY = Type.KEY,		// Public key as used in DNSSEC rfc2535

    PX = Type.PX,		// Pointer to X.400/RFC822 mail mapping information rfc2163

    GPOS = Type.GPOS,	// Geographical position rfc1712 (obsolete)

    AAAA = Type.AAAA,	// a IPV6 host address

    LOC = Type.LOC,		// Location information rfc1876

    NXT = Type.NXT,		// Obsolete rfc2065 / rfc2535

    EID = Type.EID,		// *** Endpoint Identifier (Patton)
    NIMLOC = Type.NIMLOC,// *** Nimrod Locator (Patton)

    SRV = Type.SRV,		// Location of services rfc2782

    ATMA = Type.ATMA,	// *** ATM Address (Dobrowski)

    NAPTR = Type.NAPTR,	// The Naming Authority Pointer rfc3403

    KX = Type.KX,		// Key Exchange Delegation Record rfc2230

    CERT = Type.CERT,	// *** CERT RFC2538

    A6 = Type.A6,		// IPv6 address rfc3363
    DNAME = Type.DNAME,	// A way to provide aliases for a whole domain, not just a single domain name as with CNAME. rfc2672

    SINK = Type.SINK,	// *** SINK Eastlake
    OPT = Type.OPT,		// *** OPT RFC2671

    APL = Type.APL,		// *** APL [RFC3123]

    DS = Type.DS,		// Delegation Signer rfc3658

    SSHFP = Type.SSHFP,	// *** SSH Key Fingerprint RFC-ietf-secsh-dns
    IPSECKEY = Type.IPSECKEY, // rfc4025
    RRSIG = Type.RRSIG,	// *** RRSIG RFC-ietf-dnsext-dnssec-2535
    NSEC = Type.NSEC,	// *** NSEC RFC-ietf-dnsext-dnssec-2535
    DNSKEY = Type.DNSKEY,// *** DNSKEY RFC-ietf-dnsext-dnssec-2535
    DHCID = Type.DHCID,	// rfc4701

    NSEC3 = Type.NSEC3,	// RFC5155
    NSEC3PARAM = Type.NSEC3PARAM, // RFC5155

    HIP = Type.HIP,		// RFC-ietf-hip-dns-09.txt

    SPF = Type.SPF,		// RFC4408
    UINFO = Type.UINFO,	// *** IANA-Reserved
    UID = Type.UID,		// *** IANA-Reserved
    GID = Type.GID,		// *** IANA-Reserved
    UNSPEC = Type.UNSPEC,// *** IANA-Reserved

    TKEY = Type.TKEY,	// Transaction key rfc2930
    TSIG = Type.TSIG,	// Transaction signature rfc2845

    IXFR = 251,			// incremental transfer                  [RFC1995]
    AXFR = 252,			// transfer of an entire zone            [RFC1035]
    MAILB = 253,		// mailbox-related RRs (MB, MG or MR)    [RFC1035]
    MAILA = 254,		// mail agent RRs (Obsolete - see MX)    [RFC1035]
    ANY = 255,			// A request for all records             [RFC1035]

    TA = Type.TA,		// DNSSEC Trust Authorities    [Weiler]  13 December 2005
    DLV = Type.DLV		// DNSSEC Lookaside Validation [RFC4431]
}