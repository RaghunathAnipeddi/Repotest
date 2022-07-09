using System;

namespace Chempoint.GP.Infrastructure.Extensions
{
    public static class DecimalExtensions
    {
        public static Int16 ToInt16(this double doubValue)
        {
            return Convert.ToInt16(doubValue);
        }

        public static Int32 ToInt32(this double doubValue)
        {
            return Convert.ToInt32(doubValue);
        }

        public static Int64 ToInt64(this double doubValue)
        {
            return Convert.ToInt64(doubValue);
        }
    }
}
