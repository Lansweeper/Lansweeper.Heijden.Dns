namespace TestConsoleApp.Converters
{
    public class DelimitedValuesConverter : IConverter<string>
    {
        private readonly char delimiter;
        private readonly IConverter<string, string> converter;

        public DelimitedValuesConverter(char delimiter, IConverter<string> converter) : this(delimiter, (IConverter<string, string>)converter)
        {
        }

        public DelimitedValuesConverter(char delimiter, IConverter<string, string> converter)
        {
            this.delimiter = delimiter;
            this.converter = converter;
        }

        public string Convert(string s)
        {
            if (s == null)
                return null;

            string[] splitted = s.Split(delimiter);
            for (int i = 0; i < splitted.Length; i++)
            {
                splitted[i] = converter.Convert(splitted[i]);
            }
            return string.Join(delimiter.ToString(), splitted);
        }
    }
}