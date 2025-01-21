using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Lansweeper.Heijden.Dns;
using Lansweeper.Heijden.Dns.Enums;

namespace TestConsoleApp;

internal class DnsSdClient : IDisposable
{
    /// testen
    /// 192.168.1.250, 192.168.1.93, 192.168.1.251
    /// 192.168.2.124, 192.168.2.123
    private readonly Resolver _client;

    public DnsSdClient(IPAddress ipAddress, int portNumber = 5353)
    {
        _client = new Resolver(ipAddress, portNumber);
    }

    public async Task<DnsSdData> Query(int timeOut = 2000, bool useCache = false, byte retries = 0, CancellationToken cancellationToken = default)
    {
        var details = new DnsSdData();
        var sb = new StringBuilder();
        try
        {
            _client.TimeOut = timeOut;
            _client.Retries = retries;
            _client.UseCache = useCache;

            var result = await _client.Query("_services._dns-sd._udp.local", QType.PTR, cancellationToken).ConfigureAwait(false);

            if (result.Answers == null || result.Answers.Count == 0) return details;

            foreach (var res in result.Answers)
            {
                sb.AppendLine(res.RECORD.ToString());

                var detailres = await _client.Query(res.RECORD.ToString(), QType.PTR, cancellationToken).ConfigureAwait(false);

                foreach (var resdet in detailres.Answers)
                {
                    sb.AppendLine($"\t{resdet.Type} {resdet.RECORD}");
                    ProcessOutputLine(details, res.RECORD.ToString(), resdet.Type.ToString(), resdet.RECORD.ToString());
                }

                foreach (var resdet in detailres.Additionals)
                {
                    sb.AppendLine($"\t{resdet.Type} {resdet.RECORD}");
                    ProcessOutputLine(details, res.RECORD.ToString(), resdet.Type.ToString(), resdet.RECORD.ToString());
                }
            }

            details.Success = true;
        }
        catch (Exception ex)
        {
            sb.AppendLine("Error: " + ex.Message);
        }
        finally
        {
            details.FullOutput = sb.ToString();
        }

        return details;
    }

    private static readonly HashSet<string> SvrRecordParents = new(StringComparer.InvariantCultureIgnoreCase)
    {
        "_workstation._tcp.local.", "_smb._tcp.local.", "_ftp._tcp.local.", "_http._tcp.local.",
        "_afpovertcp._tcp.local.", "_device-info._tcp.local.", "_ssh._tcp.local.",
        "_sftp-ssh._tcp.local.", "_spotify-connect._tcp.local."
    };

    private static void ProcessOutputLine(DnsSdData details, string parentPtr, string recordType,
        string detailOutput)
    {
        //Processes output lines and match to model/serial,...

            
        //extract hostname from SRV record (third number is the port name of the service)
        // SRV 0 0 9 PRDLDOCKER01.local.
        // SRV 0 0 80 NAS.local.
        // SRV 0 0 9 RT-AC56U-E580.local.
        if (string.IsNullOrEmpty(details.AssetName)
            && string.Equals(recordType, "SRV", StringComparison.InvariantCultureIgnoreCase)
            && SvrRecordParents.Contains(parentPtr))
        {
            try
            {
                var tempstr = detailOutput.Split(' ');
                details.AssetName = tempstr[3].Trim();
                if (details.AssetName.EndsWith(".local.", StringComparison.InvariantCultureIgnoreCase))
                {
                    details.AssetName = details.AssetName.Remove(details.AssetName.Length - 7);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error extracting hostname dns sd:");
                Console.WriteLine(e);
            }
        }

        //parse txt records, split by quotes
        //TXT "vendor=Synology" "model=RS815+" "serial=1760MRN646600" "version_major=6" "version_minor=1" "version_build=15266" "admin_port=5000" "secure_admin_port=5001" "mac_address=00:11:32:79:a1:55|00:11:32:79:a1:56|00:11:32:79:a1:57|00:11:32:79:a1:58"
        //TXT "model=iMac18,3" "osxvers=17"
        //TXT "rpBA=9D:D8:AF:36:EB:45" "rpVr=140.31" "rpHI=a602974ffb26" "rpHN=53cfc270f773" "rpHA=87f4ecb71bff"
        //TXT "accessType=http,accessPort=8080,model=TS-412,displayModel=TS-412,fwVer=4.3.3,fwBuildNum=20170901,serialNum=Q132B03431,webAdmPort=0,webAdmSslPort=0,webPort=0,webSslPort=0"
        //TXT "txtvers=1" "note=" "adminurl=http://BRN30055C6F279F.local./" "ty=Brother MFC-9140CDN" "mfg=Brother" "mdl=MFC-9140CDN" "button=T" "feeder=T" "flatbed=T"

        if (string.Equals(recordType, "TXT", StringComparison.InvariantCultureIgnoreCase)
            && !string.IsNullOrWhiteSpace(detailOutput) && detailOutput != "\"\"")
        {
            var dict = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);
            try
            {
                // Remove outer double quotes and split on characters: double quote and comma. Ignore empty entries.
                var parts = detailOutput.Mid(1, detailOutput.Length - 2)
                    .Split(new[] { '"', ',' }, StringSplitOptions.RemoveEmptyEntries);

                foreach (var part in parts)
                {
                    // Split each part on '=': Format is: [key]=[value]
                    var pos = part.IndexOf('=');
                    if (pos <= -1) continue;

                    details.TxtRecords.Add(part);

                    var key = part.Substring(0, pos);
                    if (!dict.ContainsKey(key))
                    {
                        dict.Add(key, part.Mid(pos + 1).Trim());
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error general getting process outputline of dns sd:");
                Console.WriteLine(e);
            }
        }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        _client?.Dispose();
    }
}