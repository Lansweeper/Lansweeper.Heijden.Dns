# Lansweeper.Heijden.Dns

Lansweeper version of the Heijden.DNS library by Alphons van der Heijden. 

See this [codeproject article](https://www.codeproject.com/Articles/23673/DNS-NET-Resolver-C) for more info.

## Lansweeper modifications
- Convert Heijden.DNS to .NET standard 2.0
- Modify RecordTXT.ToString-method
- Modify Test project to use LS logic
- Minor cleanup (added missing usings to dispose of streams and sockets)
- Fix Timeout mixup (milliseconds <> seconds)
- Fix memory leak in Resolver.UdpRequest (used an obsolete rule about the maximum size of the UDP request)
