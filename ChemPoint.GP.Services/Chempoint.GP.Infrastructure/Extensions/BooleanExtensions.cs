using System;
using System.Collections.Generic;
using System.Linq;

namespace Chempoint.GP.Infrastructure.Extensions
{
    public static class BooleanExtensions
    {
        /// <summary>
        /// True, when its value is 'true'
        /// </summary>
        /// <param name="bvalue">source bool</param>
        /// <returns>Boolean</returns>
        public static bool IsPositive(this bool bvalue)
        {
            return bvalue == true;
        }

        /// <summary>
        /// True, when its value is 'false'
        /// </summary>
        /// <param name="bvalue">source bool</param>
        /// <returns>Boolean</returns>
        public static bool IsNegative(this bool bvalue)
        {
            return !bvalue.IsPositive();
        }

        /// <summary>
        /// Covert the given object into Boolean
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static bool ToBoolean(this object obj)
        {
            if (obj == null)
                return false;

            bool result = false;

            Boolean.TryParse(obj.ToString(), out result);

            return result;
        }

        /// <summary>
        /// converts the given boolean value to string (in lower-case).
        /// </summary>
        /// <param name="bvalue">source bool</param>
        /// <returns>string equivalent</returns>
        public static string ToString(this bool bvalue)
        {
            return bvalue.ToString().ToLowerInvariant();
        }

        public static bool In<T>(this T source, params T[] list)
        {
            return list.ToList().Contains(source);
        }

        public static bool IsValidDictionary<T>(this Dictionary<string, T> dict) where T : class
        {
            if (dict != null && dict.Count > 0)
                return true;
            return false;
        }
    }
}
