namespace TestConsoleApp.Converters
{
    public class PadLeadingCharacterConverter : IConverter<string>
    {
        private readonly int _length;
        private readonly char _character;

        public PadLeadingCharacterConverter(char character, int length)
        {
            _character = character;
            _length = length;
        }

        public string Convert(string s)
        {
            if (s == null)
                return null;

            var charsToAdd = _length - s.Length;
            for (var i = 0; i < charsToAdd; i++)
            {
                s = _character + s;
            }
            return s;
        }
    }
}