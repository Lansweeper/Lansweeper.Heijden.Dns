namespace TestConsoleApp
{
    public static class StringExtensions
    {
        public static string Mid(this string input, int startIndex, int length)
        {
            var result = string.Empty;
            //do a length check first so we don't  cause an error
            //if length is longer than the string length then just get
            //the rest of the string.
            if (input == null || input.Length <= 0 || startIndex >= input.Length) return result;

            if (startIndex + length > input.Length)
            {
                length = input.Length - startIndex;
            }
            result = input.Substring(startIndex, length);
            //return the result of the operation
            return result;
        }

        public static string Mid(this string input, int startIndex)
        {
            if (input == null || input.Length <= 0 || startIndex >= input.Length) return string.Empty;

            return input.Substring(startIndex);
        }
    }
}