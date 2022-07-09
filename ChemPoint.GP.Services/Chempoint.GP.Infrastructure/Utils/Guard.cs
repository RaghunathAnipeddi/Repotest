using System;

namespace Chempoint.GP.Infrastructure.Utils
{
    public static class Guard
    {
        public static void ThrowIfNull<T>(this T o, string paramName)
            where T : class
        {
            if (o == null)
                throw new ArgumentNullException(paramName);
        }

        public static void ThrowIfNullOrEmpty(this string o, string paramName)
        {
            if (o == null || string.IsNullOrEmpty(o))
                throw new ArgumentNullException(paramName);
        }
    }
}
