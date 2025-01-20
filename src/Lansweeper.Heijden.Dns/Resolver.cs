using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using Lansweeper.Heijden.Dns.Enums;
using Lansweeper.Heijden.Dns.Records;
using Type = Lansweeper.Heijden.Dns.Enums.Type;


/*
 * Network Working Group                                     P. Mockapetris
 * Request for Comments: 1035                                           ISI
 *                                                            November 1987
 *
 *           DOMAIN NAMES - IMPLEMENTATION AND SPECIFICATION
 *
 */

namespace Lansweeper.Heijden.Dns;

/// <summary>
/// Resolver is the main class to do DNS query lookups
/// </summary>
public sealed class Resolver : IDisposable
{
    /// <summary>
    /// Version of this set of routines, when not in a library
    /// </summary>
    public string Version { get; } = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? string.Empty;

    /// <summary>
    /// Default DNS port
    /// </summary>
    private const int DefaultPort = 53;

    /// <summary>
    /// Gets list of OPENDNS servers
    /// </summary>
    public static readonly IPEndPoint[] DefaultDnsServers =
    [
        new(IPAddress.Parse("208.67.222.222"), DefaultPort), 
        new(IPAddress.Parse("208.67.220.220"), DefaultPort)
    ];

    private ushort _unique;
    private bool _useCache;

    private readonly List<IPEndPoint> _dnsServers = [];

    private readonly ReaderWriterLockSlim _responseCacheLock = new();
    private readonly Dictionary<string,Response> _responseCache = [];

    /// <summary>
    /// Constructor of Resolver using DNS servers specified.
    /// </summary>
    /// <param name="DnsServers">Set of DNS servers</param>
    public Resolver(IPEndPoint[] DnsServers)
    {
        _dnsServers.AddRange(DnsServers);

        _unique = (ushort)(Random.Shared.Next(0, ushort.MaxValue));
        Retries = 2;
        TimeOut = 1000;
        Recursion = true;
        _useCache = true;
        TransportType = TransportType.Udp;
    }

    /// <summary>
    /// Constructor of Resolver using DNS server specified.
    /// </summary>
    /// <param name="DnsServer">DNS server to use</param>
    public Resolver(IPEndPoint DnsServer)
        : this([DnsServer])
    {
    }

    /// <summary>
    /// Constructor of Resolver using DNS server and port specified.
    /// </summary>
    /// <param name="serverIpAddress">DNS server to use</param>
    /// <param name="serverPortNumber">DNS port to use</param>
    public Resolver(IPAddress serverIpAddress, int serverPortNumber)
        : this(new IPEndPoint(serverIpAddress,serverPortNumber))
    {
    }

    /// <summary>
    /// Constructor of Resolver using DNS address and port specified.
    /// </summary>
    /// <param name="serverIpAddress">DNS server address to use</param>
    /// <param name="serverPortNumber">DNS port to use</param>
    public Resolver(string serverIpAddress, int serverPortNumber)
        : this(IPAddress.Parse(serverIpAddress), serverPortNumber)
    {
    }
		
    /// <summary>
    /// Constructor of Resolver using DNS address.
    /// </summary>
    /// <param name="serverIpAddress">DNS server address to use</param>
    public Resolver(string serverIpAddress)
        : this(IPAddress.Parse(serverIpAddress), DefaultPort)
    {
    }

    /// <summary>
    /// Resolver constructor, using DNS servers specified by Windows
    /// </summary>
    public Resolver()
        : this(GetDnsServers())
    {
    }

    public class VerboseOutputEventArgs(string message) : EventArgs
    {
        public string Message { get; set; } = message;
    }

    private void Verbose(string format, params object[] args)
    {
        OnVerbose?.Invoke(this, new VerboseEventArgs(string.Format(format, args)));
    }

    /// <summary>
    /// Verbose messages from internal operations
    /// </summary>
    public event VerboseEventHandler OnVerbose;
    public delegate void VerboseEventHandler(object sender, VerboseEventArgs e);

    public class VerboseEventArgs(string message) : EventArgs
    {
        public string Message { get; set; } = message;
    }
    
    /// <summary>
    /// Gets or sets timeout in milliseconds
    /// </summary>
    public int TimeOut { get; set; }

    /// <summary>
    /// Gets or sets number of extra retries before giving up
    /// </summary>
    public byte Retries { get; set; }

    /// <summary>
    /// Gets or set recursion for doing queries
    /// </summary>
    public bool Recursion { get; set; }

    /// <summary>
    /// Gets or sets protocol to use
    /// </summary>
    public TransportType TransportType { get; set; }

    /// <summary>
    /// Gets or sets list of DNS servers to use
    /// </summary>
    public IPEndPoint[] DnsServers
    {
        get
        {
            return [.. _dnsServers];
        }
        set
        {
            _dnsServers.Clear();
            _dnsServers.AddRange(value);
        }
    }

    /// <summary>
    /// Gets first DNS server address or sets single DNS server to use
    /// </summary>
    public string DnsServer
    {
        get
        {
            return _dnsServers[0].Address.ToString();
        }
        set
        {
            if (IPAddress.TryParse(value, out var ip))
            {
                _dnsServers.Clear();
                _dnsServers.Add(new IPEndPoint(ip, DefaultPort));
                return;
            }
            var response = Query(value, QType.A);
            var recordA = response.GetRecords<RecordA>().FirstOrDefault();
            if (recordA is null) return;

            _dnsServers.Clear();
            _dnsServers.Add(new IPEndPoint(recordA.Address, DefaultPort));
        }
    }

    public bool UseCache
    {
        get
        {
            return _useCache;
        }
        set
        {
            _useCache = value;
            if (_useCache) return;

            ClearCache();
        }
    }

    /// <summary>
    /// Clear the resolver cache
    /// </summary>
    public void ClearCache()
    {
        try
        {
            _responseCacheLock.EnterWriteLock();
            _responseCache.Clear();
        }
        finally
        {
            _responseCacheLock.ExitWriteLock();
        }
    }

    private Response? SearchInCache(Question question)
    {
        if (!_useCache) return null;

        var strKey = $"{question.QClass}-{question.QType}-{question.QName}";

        Response? response = null;

        try
        {
            _responseCacheLock.EnterReadLock();
            if (_responseCache.TryGetValue(strKey, out var value))
            {
                response = value;
            }
            else
            {
                return null;
            }
        }
        finally
        {
            _responseCacheLock.ExitReadLock();
        }
        
        var timeLived = (int)((DateTime.UtcNow.Ticks - response.TimeStamp.Ticks) / TimeSpan.TicksPerSecond);
        foreach (var rr in response.GetRecordsRR())
        {
            rr.TimeLived = timeLived;
            // The TTL property calculates its actual time to live
            if (rr.TTL == 0) return null; // out of date
        }
        return response;
    }

    private void AddToCache(Response response)
    {
        if (!_useCache) return;

        // No question, no caching
        if (response.Questions.Count == 0) return;

        // Only cached non-error responses
        if (response.Header.RCODE != RCode.NoError) return;

        var question = response.Questions[0];

        var strKey = $"{question.QClass}-{question.QType}-{question.QName}";

        try
        {
            _responseCacheLock.EnterWriteLock();
            _responseCache[strKey] = response;
        }
        finally
        {
           _responseCacheLock.ExitWriteLock();
        }
    }

    private Response UdpRequest(Request request)
    {
        for (var intAttempts = 0; intAttempts <= Retries; intAttempts++)
        {
            for (var intDnsServer = 0; intDnsServer < _dnsServers.Count; intDnsServer++)
            {
                var endpoint = _dnsServers[intDnsServer];
                using var udpClient = new UdpClient(endpoint.Address.ToString(), endpoint.Port);
                udpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout, TimeOut);

                try
                {
                    udpClient.Send(request.GetData());
                    var result = udpClient.Receive(ref endpoint);
                    var response = new Response(endpoint, result);
                    AddToCache(response);
                    return response;
                }
                catch (SocketException)
                {
                    Verbose($";; Connection to nameserver {intDnsServer + 1} failed");
                    continue; // next try
                }
                finally
                {
                    _unique++;
                    // close the connection
                    udpClient.Close();
                }
            }
        }

        return new Response { Error = "Timeout Error" };
    }

    private Response TcpRequest(Request request)
    {
        for (var intAttempts = 0; intAttempts <= Retries; intAttempts++)
        {
            for (var intDnsServer = 0; intDnsServer < _dnsServers.Count; intDnsServer++)
            {
                using var tcpClient = new TcpClient();
                tcpClient.ReceiveTimeout = TimeOut;

                try
                {
                    var result = tcpClient.BeginConnect(_dnsServers[intDnsServer].Address, _dnsServers[intDnsServer].Port, null, null);

                    var success = result.AsyncWaitHandle.WaitOne(TimeOut, true);

                    if (!success || !tcpClient.Connected)
                    {
                        tcpClient.Close();
                        Verbose($";; Connection to nameserver {(intDnsServer + 1)} failed");
                        continue;
                    }

                    using var bs = new BufferedStream(tcpClient.GetStream());
                    var data = request.GetData();
                    bs.WriteByte((byte)((data.Length >> 8) & 0xff));
                    bs.WriteByte((byte)(data.Length & 0xff));
                    bs.Write(data, 0, data.Length);
                    bs.Flush();

                    var transferResponse = new Response();
                    var intSoa = 0;
                    var intMessageSize = 0;

                    //Debug.WriteLine("Sending "+ (request.Length+2) + " bytes in "+ sw.ElapsedMilliseconds+" mS");

                    while (true)
                    {
                        var intLength = bs.ReadByte() << 8 | bs.ReadByte();
                        if (intLength <= 0)
                        {
                            tcpClient.Close();
                            Verbose($";; Connection to nameserver {(intDnsServer + 1)} failed");
                            throw new SocketException(); // next try
                        }

                        intMessageSize += intLength;

                        data = new byte[intLength];
                        var bytesRead = bs.Read(data, 0, intLength);
                        var response = new Response(_dnsServers[intDnsServer], data[..bytesRead]);

                        //Debug.WriteLine("Received "+ (bytesRead+2)+" bytes in "+sw.ElapsedMilliseconds +" mS");

                        if (response.Header.RCODE != RCode.NoError)
                        {
                            return response;
                        }

                        if (response.Questions[0].QType != QType.AXFR)
                        {
                            AddToCache(response);
                            return response;
                        }

                        // Zone transfer!!

                        if (transferResponse.Questions.Count == 0)
                        {
                            transferResponse.Questions.AddRange(response.Questions);
                        }
                        transferResponse.Answers.AddRange(response.Answers);
                        transferResponse.Authorities.AddRange(response.Authorities);
                        transferResponse.Additionals.AddRange(response.Additionals);

                        if (response.Answers[0].Type == Type.SOA)
                        {
                            intSoa++;
                        }

                        if (intSoa == 2)
                        {
                            transferResponse.Header.QDCOUNT = (ushort)transferResponse.Questions.Count;
                            transferResponse.Header.ANCOUNT = (ushort)transferResponse.Answers.Count;
                            transferResponse.Header.NSCOUNT = (ushort)transferResponse.Authorities.Count;
                            transferResponse.Header.ARCOUNT = (ushort)transferResponse.Additionals.Count;
                            transferResponse.MessageSize = intMessageSize;
                            return transferResponse;
                        }
                    }
                } // try
                catch (SocketException)
                {
                    continue; // next try
                }
                finally
                {
                    _unique++;

                    // close the socket
                    tcpClient.Close();
                }
            }
        }
        return new Response { Error = "Timeout Error" };
    }

    /// <summary>
    /// Do Query on specified DNS servers
    /// </summary>
    /// <param name="name">Name to query</param>
    /// <param name="qType">Question type</param>
    /// <param name="qClass">Class type</param>
    /// <returns>Response of the query</returns>
    public Response Query(string name, QType qType, QClass qClass)
    {
        var question = new Question(name, qType, qClass);
        var response = SearchInCache(question);
        if (response is not null)
        {
            return response;
        }

        var request = new Request();
        request.AddQuestion(question);
        return GetResponse(request);
    }

    /// <summary>
    /// Do an QClass=IN Query on specified DNS servers
    /// </summary>
    /// <param name="name">Name to query</param>
    /// <param name="qType">Question type</param>
    /// <returns>Response of the query</returns>
    public Response Query(string name, QType qType)
    {
        var question = new Question(name, qType, QClass.IN);
        var response = SearchInCache(question);
        if (response is not null)
        {
            return response;
        }

        var request = new Request();
        request.AddQuestion(question);
        return GetResponse(request);
    }

    private Response GetResponse(Request request)
    {
        request.Header.ID = _unique;
        request.Header.RD = Recursion;

        return TransportType switch
        {
            TransportType.Udp => UdpRequest(request),
            TransportType.Tcp => TcpRequest(request),
            _ => new Response { Error = "Unknown TransportType" }
        };
    }

    /// <summary>
    /// Gets a list of default DNS servers used on the Windows machine.
    /// </summary>
    /// <returns></returns>
    public static IPEndPoint[] GetDnsServers()
    {
        var list = new HashSet<IPEndPoint>();

        foreach (var n in NetworkInterface.GetAllNetworkInterfaces())
        {
            if (n.OperationalStatus != OperationalStatus.Up) continue;
            
            var ipProps = n.GetIPProperties();
            foreach (var ipAddr in ipProps.DnsAddresses)
            {
                var entry = new IPEndPoint(ipAddr, DefaultPort);
                list.Add(entry);
            }
        }
        return [..list];
    } 
   
    private IPHostEntry MakeEntry(string HostName)
    {
        var entry = new IPHostEntry { HostName = HostName };

        var response = Query(HostName, QType.A, QClass.IN);

        // fill AddressList and aliases
        var addresses = new HashSet<IPAddress>();
        var aliases = new HashSet<string>();
        foreach (var answerRR in response.Answers)
        {
            if (answerRR.Type == Type.A)
            {
                // answerRR.RECORD.ToString() == (answerRR.RECORD as RecordA).Address
                addresses.Add(IPAddress.Parse((answerRR.RECORD.ToString())));
                entry.HostName = answerRR.NAME;
            }
            else
            {
                if (answerRR.Type == Type.CNAME)
                {
                    aliases.Add(answerRR.NAME);
                }
            }
        }
        entry.AddressList = [..addresses];
        entry.Aliases = [..aliases];

        return entry;
    }

    /// <summary>
    /// Translates the IPV4 or IPV6 address into an arpa address
    /// </summary>
    /// <param name="ip">IP address to get the arpa address form</param>
    /// <returns>The 'mirrored' IPV4 or IPV6 arpa address</returns>
    public static string GetArpaFromIp(IPAddress ip)
    {
        switch (ip.AddressFamily)
        {
            case AddressFamily.InterNetwork:
            {
                var sb = new StringBuilder("in-addr.arpa.");
                foreach (var b in ip.GetAddressBytes())
                {
                    sb.Insert(0, $"{b}.");
                }
                return sb.ToString();
            }
            case AddressFamily.InterNetworkV6:
            {
                var sb = new StringBuilder("ip6.arpa.");
                foreach (var b in ip.GetAddressBytes())
                {
                    sb.Insert(0, $"{(b >> 4) & 0xf:x}.");
                    sb.Insert(0, $"{(b >> 0) & 0xf:x}.");
                }
                return sb.ToString();
            }
            default:
                return "?";
        }
    }

    public static string GetArpaFromEnum(string strEnum)
    {
        var sb = new StringBuilder("e164.arpa.");
        foreach (var c in strEnum.Where(c => char.IsBetween(c, '0', '9')))
        {
            sb.Insert(0, $"{c}.");
        }
        return sb.ToString();
    }

    /// <summary>
    ///		Resolves an IP address to an System.Net.IPHostEntry instance.
    /// </summary>
    /// <param name="ip">An IP address.</param>
    /// <returns>
    ///		An System.Net.IPHostEntry instance that contains address information about
    ///		the host specified in address.
    ///</returns>
    public IPHostEntry GetHostEntry(IPAddress ip)
    {
        var response = Query(GetArpaFromIp(ip), QType.PTR, QClass.IN);
        var recordPTR = response.GetRecords<RecordPTR>().FirstOrDefault();
        return recordPTR is null 
            ? new IPHostEntry() 
            : MakeEntry(recordPTR.PTRDNAME);
    }

    /// <summary>
    ///		Resolves a host name or IP address to an System.Net.IPHostEntry instance.
    /// </summary>
    /// <param name="hostNameOrAddress">The host name or IP address to resolve.</param>
    /// <returns>
    ///		An System.Net.IPHostEntry instance that contains address information about
    ///		the host specified in hostNameOrAddress. 
    ///</returns>
    public IPHostEntry GetHostEntry(string hostNameOrAddress)
    {
        return IPAddress.TryParse(hostNameOrAddress, out var iPAddress) ? GetHostEntry(iPAddress) : MakeEntry(hostNameOrAddress);
    }


    private enum RRRecordStatus
    {
        UNKNOWN,
        NAME,
        TTL,
        CLASS,
        TYPE,
        VALUE
    }

    public void LoadRootFile(string strPath)
    {
        using var sr = new StreamReader(strPath);
        while (!sr.EndOfStream)
        {
            var strLine = sr.ReadLine();
            if (strLine is null) break;

            var intI = strLine.IndexOf(';');
            if (intI >= 0)
            {
                strLine = strLine[..intI];
            }
            strLine = strLine.Trim();
            if (strLine.Length == 0) continue;

            var status = RRRecordStatus.NAME;
            var name = string.Empty;
            var ttl = string.Empty;
            var Class = string.Empty;
            var type=string.Empty;
            var value = string.Empty;
            var strW = string.Empty;

            for (intI = 0; intI < strLine.Length; intI++)
            {
                var c = strLine[intI];

                if (c <= ' ' && strW != string.Empty)
                {
                    switch (status)
                    {
                        case RRRecordStatus.NAME:
                            name = strW;
                            status = RRRecordStatus.TTL;
                            break;
                        case RRRecordStatus.TTL:
                            ttl = strW;
                            status = RRRecordStatus.CLASS;
                            break;
                        case RRRecordStatus.CLASS:
                            Class = strW;
                            status = RRRecordStatus.TYPE;
                            break;
                        case RRRecordStatus.TYPE:
                            type = strW;
                            status = RRRecordStatus.VALUE;
                            break;
                        case RRRecordStatus.VALUE:
                            value = strW;
                            status = RRRecordStatus.UNKNOWN;
                            break;
                        default:
                            break;
                    }
                    strW = string.Empty;
                }

                if (c > ' ')
                {
                    strW += c;
                }
            }

        }
        sr.Close();
    }

    /// <inheritdoc />
    public void Dispose()
    {
        _responseCacheLock.Dispose();
    }
}