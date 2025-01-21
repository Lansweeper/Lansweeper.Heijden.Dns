# Lansweeper.Heijden.Dns

Lansweeper version of the Heijden.DNS library by Alphons van der Heijden. 

See this [codeproject article](https://www.codeproject.com/Articles/23673/DNS-NET-Resolver-C) for more info.

## Lansweeper modifications
- Modify Test project to use LS logic
- Cleanup:
	- added missing usings to dispose of streams and sockets
	- Code formatting + naming conventions
- Fix Timeout mixup (milliseconds <> seconds)
- Fix Retries mixup (Retries <> Attempts)
- Fix memory leak in Resolver.UdpRequest (used an obsolete rule about the maximum size of the UDP request)
- Upgrade to .NET 8
- Made Query-methods async and use a cancellationtoken