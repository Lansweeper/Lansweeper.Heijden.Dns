using System;
using System.Net;
using TestConsoleApp;

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
