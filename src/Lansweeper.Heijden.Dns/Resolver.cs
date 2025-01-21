using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using Lansweeper.Heijden.Dns.Enums;
using Lansweeper.Heijden.Dns.Records;
using Type = Lansweeper.Heijden.Dns.Enums.Type;

namespace Lansweeper.Heijden.Dns;

/*
 * Network Working Group                                     P. Mockapetris
 * Request for Comments: 1035                                           ISI
 *                                                            November 1987
 *
 *           DOMAIN NAMES - IMPLEMENTATION AND SPECIFICATION
 *
 */

/// <summary>
/// Resolver is the main class to do DNS query lookups
/// </summary>
public sealed class Resolver : IResolver
{
    /// <inheritdoc />
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
    /// Constructor of Resolver using DNS server and port specified.
    /// </summary>
    /// <param name="serverIpAddress">DNS server to use</param>
    /// <param name="serverPortNumber">DNS port to use</param>
    public Resolver(IPAddress serverIpAddress, int serverPortNumber = DefaultPort)
        : this([new IPEndPoint(serverIpAddress,serverPortNumber)])
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
    public event VerboseEventHandler? OnVerbose;
    public delegate void VerboseEventHandler(object sender, VerboseEventArgs e);

    public class VerboseEventArgs(string message) : EventArgs
    {
        public string Message { get; set; } = message;
    }
    
    /// <inheritdoc />
    public int TimeOut { get; set; }

    /// <inheritdoc />
    public byte Retries { get; set; }

    /// <inheritdoc />
    public bool Recursion { get; set; }

    /// <inheritdoc />
    public TransportType TransportType { get; set; }

    /// <inheritdoc />
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

    /// <inheritdoc />
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

            Task.Factory.StartNew(async () =>
            {
                var response = await Query(value, QType.A);
                var recordA = response.GetRecordsOfType<RecordA>().FirstOrDefault();
                if (recordA is null) return;

                _dnsServers.Clear();
                _dnsServers.Add(new IPEndPoint(recordA.Address, DefaultPort));
            });
        }
    }

    /// <inheritdoc />
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

    /// <inheritdoc />
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

        var strKey = $"{question.Class}-{question.Type}-{question.Name}";

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
        foreach (var rr in response.GetResourceRecord())
        {
            rr.TimeLived = timeLived;
            // The TTL property calculates its actual time to live
            if (rr.Ttl == 0) return null; // out of date
        }
        return response;
    }

    private void AddToCache(Response response)
    {
        if (!_useCache) return;

        // No question, no caching
        if (response.Questions.Count == 0) return;

        // Only cached non-error responses
        if (response.Header.ResponseCode != RCode.NoError) return;

        var question = response.Questions[0];

        var strKey = $"{question.Class}-{question.Type}-{question.Name}";

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

    private async Task<Response> UdpRequest(Request request, CancellationToken cancellationToken)
    {
        for (var intAttempts = 0; intAttempts <= Retries; intAttempts++)
        {
            for (var intDnsServer = 0; intDnsServer < _dnsServers.Count; intDnsServer++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var endpoint = _dnsServers[intDnsServer];
                using var udpClient = new UdpClient(endpoint.Address.ToString(), endpoint.Port);
                udpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout, TimeOut);

                try
                {
                    await TimeoutHelper.ExecuteAsyncWithTimeout(async ct => await udpClient.SendAsync(request.GetData(), ct), TimeOut, cancellationToken)
                        .ConfigureAwait(false);
                    var result = await TimeoutHelper.ExecuteAsyncWithTimeout(async ct => await udpClient.ReceiveAsync(ct), TimeOut, cancellationToken).ConfigureAwait(false);
                    var response = new Response(endpoint, result.Buffer);
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

    private async Task<Response> TcpRequest(Request request, CancellationToken cancellationToken)
    {
        for (var intAttempts = 0; intAttempts <= Retries; intAttempts++)
        {
            for (var intDnsServer = 0; intDnsServer < _dnsServers.Count; intDnsServer++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                using var tcpClient = new TcpClient();
                tcpClient.ReceiveTimeout = TimeOut;

                try
                {
                    await TimeoutHelper.ExecuteAsyncWithTimeout(async ct => await tcpClient.ConnectAsync(_dnsServers[intDnsServer].Address, _dnsServers[intDnsServer].Port, ct), TimeOut, cancellationToken)
                        .ConfigureAwait(false);
                    
                    if (!tcpClient.Connected)
                    {
                        tcpClient.Close();
                        Verbose($";; Connection to nameserver {(intDnsServer + 1)} failed");
                        continue;
                    }

                    await using var bs = new BufferedStream(tcpClient.GetStream());
                    var data = request.GetData();
                    bs.WriteByte((byte)((data.Length >> 8) & 0xff));
                    bs.WriteByte((byte)(data.Length & 0xff));
                    await bs.WriteAsync(data.AsMemory(), cancellationToken).ConfigureAwait(false);
                    await bs.FlushAsync(cancellationToken).ConfigureAwait(false);

                    var transferResponse = new Response();
                    var intSoa = 0;
                    var intMessageSize = 0;

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
                        var bytesRead = await TimeoutHelper.ExecuteAsyncWithTimeout(async ct => await bs.ReadAsync(data.AsMemory(), ct), TimeOut, cancellationToken)
                                        .ConfigureAwait(false);
                        
                        var response = new Response(_dnsServers[intDnsServer], data[..bytesRead]);
                        if (response.Header.ResponseCode != RCode.NoError)
                        {
                            return response;
                        }

                        if (response.Questions[0].Type != QType.AXFR)
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
                            transferResponse.Header.QuestionCount = (ushort)transferResponse.Questions.Count;
                            transferResponse.Header.AnswerCount = (ushort)transferResponse.Answers.Count;
                            transferResponse.Header.NameServerCount = (ushort)transferResponse.Authorities.Count;
                            transferResponse.Header.AdditionRecordCount = (ushort)transferResponse.Additionals.Count;
                            transferResponse.MessageSize = intMessageSize;
                            return transferResponse;
                        }
                    }
                } 
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

    /// <inheritdoc />
    public async Task<Response> Query(string name, QType qType, QClass qClass, CancellationToken cancellationToken = default)
    {
        var question = new Question(name, qType, qClass);
        var response = SearchInCache(question);
        if (response is not null)
        {
            return response;
        }

        var request = new Request();
        request.AddQuestion(question);
        return await GetResponse(request, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<Response> Query(string name, QType qType, CancellationToken cancellationToken = default)
    {
        var question = new Question(name, qType, QClass.IN);
        var response = SearchInCache(question);
        if (response is not null)
        {
            return response;
        }

        var request = new Request();
        request.AddQuestion(question);
        return await GetResponse(request, cancellationToken).ConfigureAwait(false);
    }

    private async Task<Response> GetResponse(Request request, CancellationToken cancellationToken)
    {
        request.Header.Id = _unique;
        request.Header.RecursionDesired = Recursion;

        return TransportType switch
        {
            TransportType.Udp => await UdpRequest(request, cancellationToken),
            TransportType.Tcp => await TcpRequest(request, cancellationToken),
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
   
    private async Task<IPHostEntry> MakeEntry(string hostName, CancellationToken cancellationToken)
    {
        var entry = new IPHostEntry { HostName = hostName };

        var response = await Query(hostName, QType.A, QClass.IN, cancellationToken).ConfigureAwait(false);

        // fill AddressList and aliases
        var addresses = new HashSet<IPAddress>();
        var aliases = new HashSet<string>();
        foreach (var answerRR in response.Answers)
        {
            if (answerRR.Record is RecordA recordA)
            {
                addresses.Add(recordA.Address);
                entry.HostName = answerRR.Name;
            }
            else if (answerRR.Type == Type.CNAME)
            {
                aliases.Add(answerRR.Name);
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

    /// <inheritdoc />
    public async Task<IPHostEntry> GetHostEntry(IPAddress ip, CancellationToken cancellationToken = default)
    {
        var response = await Query(GetArpaFromIp(ip), QType.PTR, QClass.IN, cancellationToken).ConfigureAwait(false);
        var recordPTR = response.GetRecordsOfType<RecordPTR>().FirstOrDefault();
        return recordPTR is null 
            ? new IPHostEntry() 
            : await MakeEntry(recordPTR.Name, cancellationToken).ConfigureAwait(false);
    }

    //// <inheritdoc />
    public async Task<IPHostEntry> GetHostEntry(string hostNameOrAddress, CancellationToken cancellationToken = default)
    {
        return IPAddress.TryParse(hostNameOrAddress, out var iPAddress) 
            ? await GetHostEntry(iPAddress, cancellationToken).ConfigureAwait(false)
            : await MakeEntry(hostNameOrAddress, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        _responseCacheLock.Dispose();
    }
}