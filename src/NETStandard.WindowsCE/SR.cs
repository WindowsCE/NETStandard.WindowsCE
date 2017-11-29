partial class SR
{
    public const string AggregateException_ToString = "{0}{1}---> (Inner Exception #{2}) {3}{4}{5}";

    public static string Format(string format, params object[] args)
    {
        return string.Format(format, args);
    }
}
