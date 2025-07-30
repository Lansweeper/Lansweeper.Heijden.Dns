using System.Net;
using Lansweeper.Heijden.Dns.Enums;

namespace Lansweeper.Heijden.Dns;

public interface IResolver : IDisposable
{
    /// <summary>
    /// Version of this set of routines, when not in a library
    /// </summary>
    string Version { get; }

    /// <summary>
    /// Gets or sets timeout in milliseconds
    /// </summary>
    int TimeOut { get; set; }

    /// <summary>
    /// Gets or sets number of extra attempts before giving up
    /// </summary>
    byte Retries { get; set; }

    /// <summary>
    /// Gets or set recursion for doing queries
    /// </summary>
    bool Recursion { get; set; }

    /// <summary>
    /// Gets or sets protocol to use
    /// </summary>
    TransportType TransportType { get; set; }

    /// <summary>
    /// By providing a local address, you specify which network interface to use for the connection.
    /// </summary>
    IPAddress? LocalAddress { get; set; }

    /// <summary>
    /// Gets or sets the network namespace to use for the connection.
    /// Only supported on Linux.
    /// </summary>
    string? NetworkNamespace { get; set; }

    /// <summary>
    /// Gets or sets list of DNS servers to use
    /// </summary>
    IPEndPoint[] DnsServers { get; set; }

    /// <summary>
    /// Gets first DNS server address or sets single DNS server to use
    /// </summary>
    string DnsServer { get; set; }

    bool UseCache { get; set; }

    /// <summary>
    /// Verbose messages from internal operations
    /// </summary>
    event Resolver.VerboseEventHandler? OnVerbose;

    /// <summary>
    /// Clear the resolver cache
    /// </summary>
    void ClearCache();

    /// <summary>
    /// Do Query on specified DNS servers
    /// </summary>
    /// <param name="name">Name to query</param>
    /// <param name="qType">Question type</param>
    /// <param name="qClass">Class type</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Response of the query</returns>
    Task<Response> Query(string name, QType qType, QClass qClass, CancellationToken cancellationToken = default);

    /// <summary>
    /// Do an QClass=IN Query on specified DNS servers
    /// </summary>
    /// <param name="name">Name to query</param>
    /// <param name="qType">Question type</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Response of the query</returns>
    Task<Response> Query(string name, QType qType, CancellationToken cancellationToken = default);

    ///  <summary>
    /// 		Resolves an IP address to an System.Net.IPHostEntry instance.
    ///  </summary>
    ///  <param name="ip">An IP address.</param>
    ///  <param name="cancellationToken">Cancellation token</param>
    ///  <returns>
    /// 		An System.Net.IPHostEntry instance that contains address information about
    /// 		the host specified in address.
    /// </returns>
    Task<IPHostEntry> GetHostEntry(IPAddress ip, CancellationToken cancellationToken = default);

    ///  <summary>
    /// 		Resolves a host name or IP address to an System.Net.IPHostEntry instance.
    ///  </summary>
    ///  <param name="hostNameOrAddress">The host name or IP address to resolve.</param>
    ///  <param name="cancellationToken">Cancellation token</param>
    ///  <returns>
    /// 		An System.Net.IPHostEntry instance that contains address information about
    /// 		the host specified in hostNameOrAddress. 
    /// </returns>
    Task<IPHostEntry> GetHostEntry(string hostNameOrAddress, CancellationToken cancellationToken = default);
}