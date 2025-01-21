using System.Collections.Generic;

namespace TestConsoleApp
{
    public class DnsSdData
    {
        public string AssetName { get; set; }

        public string FullOutput { get; set; }
        public bool Success { get; set; }

        public HashSet<string> TxtRecords { get; set; } = new();
    }
}