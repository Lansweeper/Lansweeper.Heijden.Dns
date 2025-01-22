using System.Net;
using System.Text;

namespace Lansweeper.Heijden.Dns;

internal static class RecordWriter
{
    internal static byte[] WriteName(string input)
    {
        if (!input.EndsWith('.'))
        {
            input += '.';
        }

        if (input == ".")
        {
            return new byte[1];
        }

        var sb = new StringBuilder();
        int i, j, len = input.Length;
        sb.Append('\0');
        for (i = 0, j = 0; i < len; i++, j++)
        {
            sb.Append(input[i]);
            if (input[i] == '.')
            {
                sb[i - j] = (char)(j & 0xff);
                j = -1;
            }
        }
        sb[^1] = '\0';
        return Encoding.ASCII.GetBytes(sb.ToString());
    }

    internal static byte[] WriteShort(ushort value)
    {
        return BitConverter.GetBytes(IPAddress.HostToNetworkOrder((short)value));
    }
    
    internal static byte[] WriteInt(uint value)
    {
        return BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)value));
    }
}