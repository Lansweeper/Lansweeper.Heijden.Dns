using System.Linq;

namespace TestConsoleApp.Converters
{
    public static class MacAddressConverter 
    {
        private static readonly IConverter<string> LeadingZeroesConverter = new DelimitedValuesConverter(':', new PadLeadingCharacterConverter('0', 2));

        public static string Convert(string macAddress)
        {
            if (macAddress == null)
                return null;

            if (macAddress.Length == 6)
            {
                macAddress = string.Join(":", macAddress.Select(c => ((int)c).ToString("x")));
            }

            macAddress = macAddress.Trim();
            macAddress = macAddress.Replace('.', ':').Replace(' ', ':').Replace('-', ':');

            macAddress = LeadingZeroesConverter.Convert(macAddress);

            if (macAddress.Length == 12 && !macAddress.Contains(':'))
            {
                macAddress =
                    macAddress.Substring(0, 2) + ":" +
                    macAddress.Substring(2, 2) + ":" +
                    macAddress.Substring(4, 2) + ":" +
                    macAddress.Substring(6, 2) + ":" +
                    macAddress.Substring(8, 2) + ":" +
                    macAddress.Substring(10, 2);
            }
            else if (macAddress.Length != 17 || macAddress[2] != ':')
            {
                return null;
            }

            return macAddress.ToUpperInvariant();
        }
    }
}