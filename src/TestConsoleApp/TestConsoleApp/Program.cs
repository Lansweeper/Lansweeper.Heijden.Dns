using System;
using System.Net;
using Lansweeper.Heijden.Dns;
using Lansweeper.Heijden.Dns.Enums;
using Lansweeper.Heijden.Dns.Records;
using TestConsoleApp;

/*
var domain = "ls.local";
var resolver = new Resolver();
var response = await resolver.Query(domain, QType.ANY, QClass.IN);

Console.WriteLine($"Querying DNS records for domain '{domain}':");
var nameEnd = '.' + domain + '.';
foreach (var record in response.Additionals)
{
    if (record.Record is RecordA ar && ar.ResourceRecord.Name.EndsWith(nameEnd))
    {
        Console.WriteLine($"{ar.Address} {ar.ResourceRecord.Name[0..^nameEnd.Length]}");
    }
}
*/


var client = new DnsSdClient(IPAddress.Parse("192.168.111.90"));
var result = await client.Query();

Console.WriteLine("TXT records");
foreach (var txtRecord in result.TxtRecords)
{
    Console.WriteLine(txtRecord);
}

Console.WriteLine();

Console.WriteLine("Full output:");
Console.WriteLine(result.FullOutput);

Console.WriteLine();
Console.WriteLine("Done");

Console.ReadKey();
