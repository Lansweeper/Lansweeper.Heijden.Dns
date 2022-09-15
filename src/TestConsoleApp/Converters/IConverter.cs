namespace TestConsoleApp.Converters
{
    public interface IConverter<in Tin, out Tout>
    {
        Tout Convert(Tin obj);
    }

    public interface IConverter<T> : IConverter<T, T>
    {
    }
}
