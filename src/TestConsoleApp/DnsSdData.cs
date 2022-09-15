using System.Collections.Generic;

namespace TestConsoleApp
{
    public class DnsSdData
    {
        public List<string> MacAddresses { get; set; } = new();

        public string Model { get; set; }

        public string SerialNumber { get; set; }

        public string Manufacturer { get; set; }

        public string AssetName { get; set; }

        public string FullOutput { get; set; }
        public bool Success { get; set; }

        public HashSet<string> TxtRecords { get; set; } = new();
    }
}