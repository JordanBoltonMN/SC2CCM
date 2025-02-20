public static class ObjectExtensions
{
    public static string ToTraceableString(this object obj)
    {
        return obj is null ? "[null]" : obj.ToString();
    }
}
