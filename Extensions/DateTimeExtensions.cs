namespace RuminsterBackend.Extensions
{
    public static class DateTimeExtensions
    {
        public static DateTime SafeToUniversalTime(this DateTime dt)
        {
            if (dt.Kind == DateTimeKind.Unspecified)
            {
                throw new Exception("DateTime Kind is Unspecified, conversion to UTC unsafe");
            }

            return dt.ToUniversalTime();
        }
    }
}
