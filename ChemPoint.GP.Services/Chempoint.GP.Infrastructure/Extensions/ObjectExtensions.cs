using System;

namespace Chempoint.GP.Infrastructure.Extensions
{
    public static class ObjectExtensions
    {
        public static bool IsOfType<T>(this object obj)
        {
            return obj.GetType() == typeof(T);
        }

        public static bool IsNotOfType<T>(this object obj)
        {
            return !obj.IsOfType<T>();
        }

        public static bool IsValid(this object obj)
        {
            return !obj.IsInValid();
        }

        public static bool IsInValid(this object obj)
        {
            return obj == null;
        }

        public static T ConvertTo<T>(this object value)
        {
            if (value == null)
                return default(T);

            try
            {
                return (T)Convert.ChangeType(value, typeof(T));
            }
            catch
            {
                return default(T);
            }
        }

        public static T CastTo<T>(this object value)
        {
            return value != DBNull.Value ? (T)value : default(T);
        }
    }
}
